package iskypro

import (
	"bufio"
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"math"
	"os"
	"strconv"
	"strings"
	"sync"
	"sync/atomic"
	"time"
)

const (
	DefaultSdkVersion     = SDKVersion
	JsonRpcVersion        = "2.0"
	ProtocolVersion       = 2
	DefaultRequestTimeout = 30 * time.Second
	MaxHeaderLength       = 8 * 1024
	MaxPayloadLength      = 4 * 1024 * 1024
	jsonRpcMethodNotFound = -32601
	jsonRpcInvalidRequest = -32600
	jsonRpcInvalidParams  = -32602
	jsonRpcPluginError    = -32000
)

type JsonObject map[string]any

type UserRef struct {
	Provider     string
	MentionID    string
	UserOpenID   string
	MemberOpenID string
	UnionOpenID  string
	DisplayName  string
}

type MessagePart interface {
	appendWire([]JsonObject) ([]JsonObject, error)
}

type QqBotMentionFormat string

const (
	QqBotMentionFormatCurrent    QqBotMentionFormat = "current"
	QqBotMentionFormatLegacy     QqBotMentionFormat = "legacy"
	QqBotMentionFormatLegacyBang QqBotMentionFormat = "legacy-bang"
)

type textPart struct {
	text string
}

func Text(value string) MessagePart {
	return textPart{text: value}
}

func (p textPart) appendWire(parts []JsonObject) ([]JsonObject, error) {
	return append(parts, JsonObject{"type": "text", "text": p.text}), nil
}

type mentionPart struct {
	target      string
	id          string
	qqBotFormat QqBotMentionFormat
}

func AtUser(id string) MessagePart {
	return mentionPart{target: "user", id: id}
}

func AtUserWithQqBotFormat(id string, format QqBotMentionFormat) MessagePart {
	return mentionPart{target: "user", id: id, qqBotFormat: format}
}

func AtUserRef(user UserRef) MessagePart {
	return AtUser(user.MentionID)
}

func AtUserRefWithQqBotFormat(user UserRef, format QqBotMentionFormat) MessagePart {
	return AtUserWithQqBotFormat(user.MentionID, format)
}

func AtEveryone() MessagePart {
	return mentionPart{target: "everyone"}
}

func (p mentionPart) appendWire(parts []JsonObject) ([]JsonObject, error) {
	if p.target == "user" {
		if err := validateID(p.id, "mention id"); err != nil {
			return nil, err
		}
		if p.qqBotFormat != "" && p.qqBotFormat != QqBotMentionFormatCurrent && p.qqBotFormat != QqBotMentionFormatLegacy && p.qqBotFormat != QqBotMentionFormatLegacyBang {
			return nil, errors.New("QQBot mention format must be current, legacy, or legacy-bang")
		}
		if (p.qqBotFormat == QqBotMentionFormatLegacy || p.qqBotFormat == QqBotMentionFormatLegacyBang) && strings.ContainsAny(p.id, "<>") {
			return nil, errors.New("legacy QQBot mention ids must not contain '<' or '>'")
		}
		wire := JsonObject{"type": "mention", "target": "user", "id": p.id}
		if p.qqBotFormat != "" {
			wire["qqBotFormat"] = string(p.qqBotFormat)
		}
		return append(parts, wire), nil
	}
	if p.target == "everyone" {
		return append(parts, JsonObject{"type": "mention", "target": "everyone"}), nil
	}
	return nil, errors.New("unsupported mention target")
}

type compositePart struct {
	parts []MessagePart
}

func AtUsers(ids ...string) MessagePart {
	parts := make([]MessagePart, 0, len(ids)*2)
	for index, id := range ids {
		if index > 0 {
			parts = append(parts, Text(" "))
		}
		parts = append(parts, AtUser(id))
	}
	return compositePart{parts: parts}
}

func AtUserRefs(users ...UserRef) MessagePart {
	parts := make([]MessagePart, 0, len(users)*2)
	for index, user := range users {
		if index > 0 {
			parts = append(parts, Text(" "))
		}
		parts = append(parts, AtUserRef(user))
	}
	return compositePart{parts: parts}
}

func AtUsersWithQqBotFormat(format QqBotMentionFormat, ids ...string) MessagePart {
	parts := make([]MessagePart, 0, len(ids)*2)
	for index, id := range ids {
		if index > 0 {
			parts = append(parts, Text(" "))
		}
		parts = append(parts, AtUserWithQqBotFormat(id, format))
	}
	return compositePart{parts: parts}
}

func AtUserRefsWithQqBotFormat(format QqBotMentionFormat, users ...UserRef) MessagePart {
	parts := make([]MessagePart, 0, len(users)*2)
	for index, user := range users {
		if index > 0 {
			parts = append(parts, Text(" "))
		}
		parts = append(parts, AtUserRefWithQqBotFormat(user, format))
	}
	return compositePart{parts: parts}
}

func (p compositePart) appendWire(parts []JsonObject) ([]JsonObject, error) {
	if len(p.parts) == 0 {
		return nil, errors.New("AtUsers requires at least one user")
	}
	if (len(p.parts)+1)/2 > 20 {
		return nil, errors.New("AtUsers supports at most 20 users")
	}
	var err error
	for _, child := range p.parts {
		parts, err = child.appendWire(parts)
		if err != nil {
			return nil, err
		}
	}
	return parts, nil
}

type MessageContext struct {
	EventID      string
	EventType    string
	Timestamp    any
	Source       string
	Bot          JsonObject
	Conversation JsonObject
	Sender       UserRef
	ID           string
	Text         string
	Attachments  []any
	Mentions     []UserRef
	RawPayload   JsonObject

	reference JsonObject
	context   *PluginContext
}

func (m *MessageContext) Reply(ctx context.Context, parts ...MessagePart) (any, error) {
	message, err := normalizeMessage(parts)
	if err != nil {
		return nil, err
	}
	return m.context.Invoke(ctx, "messages.reply", JsonObject{
		"reference": m.reference,
		"message":   message,
	})
}

type MessageService struct {
	context *PluginContext
}

func (s MessageService) Group(id string) MessageTarget {
	return MessageTarget{context: s.context, target: JsonObject{"type": "group", "id": id}}
}

func (s MessageService) Channel(id string) MessageTarget {
	return MessageTarget{context: s.context, target: JsonObject{"type": "channel", "id": id}}
}

func (s MessageService) User(id string) MessageTarget {
	return MessageTarget{context: s.context, target: JsonObject{"type": "user", "id": id}}
}

func (s MessageService) DirectMessage(id string) MessageTarget {
	return MessageTarget{context: s.context, target: JsonObject{"type": "direct", "id": id}}
}

type MessageTarget struct {
	context *PluginContext
	target  JsonObject
}

func (t MessageTarget) Send(ctx context.Context, parts ...MessagePart) (any, error) {
	if err := validateID(stringValue(t.target["id"]), "message target id"); err != nil {
		return nil, err
	}
	message, err := normalizeMessage(parts)
	if err != nil {
		return nil, err
	}
	return t.context.Invoke(ctx, "messages.send", JsonObject{
		"target":  t.target,
		"message": message,
	})
}

func normalizeMessage(parts []MessagePart) (JsonObject, error) {
	wire := []JsonObject{}
	var err error
	for _, part := range parts {
		if part == nil {
			return nil, errors.New("message parts must not contain nil")
		}
		wire, err = part.appendWire(wire)
		if err != nil {
			return nil, err
		}
	}
	normalized := []JsonObject{}
	for _, part := range wire {
		if stringValue(part["type"]) == "text" && stringValue(part["text"]) == "" {
			continue
		}
		if stringValue(part["type"]) == "text" && len(normalized) > 0 && stringValue(normalized[len(normalized)-1]["type"]) == "text" {
			normalized[len(normalized)-1]["text"] = stringValue(normalized[len(normalized)-1]["text"]) + stringValue(part["text"])
			continue
		}
		normalized = append(normalized, part)
	}
	if len(normalized) == 0 {
		return nil, errors.New("message must contain at least one non-empty part")
	}
	return JsonObject{"parts": normalized}, nil
}

func validateID(value string, label string) error {
	if strings.TrimSpace(value) == "" {
		return fmt.Errorf("%s must not be empty", label)
	}
	for _, character := range value {
		if character < 32 || character == 127 {
			return fmt.Errorf("%s must not contain control characters", label)
		}
	}
	return nil
}

type SdkMethod struct {
	Name           string
	Permission     string
	Stability      string
	RequestModel   string
	ResponseModel  string
	DefaultEnabled bool
}

type EventAck struct {
	Accepted bool
	EventID  string
	Error    any
}

type EventHandler func(context.Context, *MessageContext, *PluginContext) (EventAck, error)

type JsonRpcError struct {
	Code    int
	Message string
	Data    any
}

func (e *JsonRpcError) Error() string {
	return e.Message
}

type rpcResponse struct {
	result any
	err    error
}

type rpcConnection struct {
	input  *bufio.Reader
	output *bufio.Writer
	token  string

	writeGate      sync.Mutex
	pendingGate    sync.Mutex
	pending        map[string]chan rpcResponse
	nextRequestID  atomic.Int64
	requestTimeout time.Duration
	closed         chan struct{}
	closeOnce      sync.Once
	closeErr       error
}

func newRpcConnection(input *bufio.Reader, output *bufio.Writer) *rpcConnection {
	connection := &rpcConnection{
		input:          input,
		output:         output,
		pending:        map[string]chan rpcResponse{},
		requestTimeout: DefaultRequestTimeout,
		closed:         make(chan struct{}),
	}
	connection.nextRequestID.Store(1000)
	return connection
}

func (c *rpcConnection) request(ctx context.Context, method string, parameters JsonObject) (any, error) {
	if err := ctx.Err(); err != nil {
		return nil, err
	}
	if strings.TrimSpace(method) == "" {
		return nil, errors.New("SDK method is required")
	}

	requestParameters := JsonObject{}
	for key, value := range parameters {
		requestParameters[key] = value
	}
	requestParameters["token"] = c.token

	id := c.nextRequestID.Add(1)
	key := requestKey(id)
	completion := make(chan rpcResponse, 1)
	c.pendingGate.Lock()
	if _, exists := c.pending[key]; exists {
		c.pendingGate.Unlock()
		return nil, fmt.Errorf("duplicate JSON-RPC request id: %d", id)
	}
	c.pending[key] = completion
	c.pendingGate.Unlock()
	defer func() {
		c.pendingGate.Lock()
		delete(c.pending, key)
		c.pendingGate.Unlock()
	}()

	if err := c.write(JsonObject{
		"jsonrpc": JsonRpcVersion,
		"id":      id,
		"method":  method,
		"params":  requestParameters,
	}); err != nil {
		return nil, err
	}

	timeout := c.requestTimeout
	if timeout <= 0 || timeout > 5*time.Minute {
		timeout = DefaultRequestTimeout
	}
	timer := time.NewTimer(timeout)
	defer timer.Stop()

	select {
	case <-ctx.Done():
		return nil, ctx.Err()
	case <-c.closed:
		if c.closeErr != nil {
			return nil, c.closeErr
		}
		return nil, io.EOF
	case <-timer.C:
		return nil, fmt.Errorf("SDK request %q timed out after %d ms", method, timeout.Milliseconds())
	case response := <-completion:
		return response.result, response.err
	}
}

func (c *rpcConnection) routeResponse(message JsonObject) bool {
	if _, hasResult := message["result"]; !hasResult {
		if _, hasError := message["error"]; !hasError {
			return false
		}
	}
	key := requestKey(message["id"])
	c.pendingGate.Lock()
	completion := c.pending[key]
	c.pendingGate.Unlock()
	if completion == nil {
		return true
	}

	response := rpcResponse{result: message["result"]}
	if errorValue, exists := message["error"]; exists {
		response.err = parseJsonRpcError(errorValue)
	}
	select {
	case completion <- response:
	default:
	}
	return true
}

func (c *rpcConnection) write(message JsonObject) error {
	c.writeGate.Lock()
	defer c.writeGate.Unlock()
	return WriteFrame(c.output, message)
}

func (c *rpcConnection) closeWithError(err error) {
	c.closeOnce.Do(func() {
		c.closeErr = err
		close(c.closed)
	})
}

type PluginContext struct {
	PluginID string
	Messages MessageService

	connection *rpcConnection
}

func newRuntimePluginContext(pluginID string, connection *rpcConnection) *PluginContext {
	context := &PluginContext{
		PluginID:   pluginID,
		connection: connection,
	}
	context.Messages = MessageService{context: context}
	return context
}

func (c *PluginContext) Invoke(ctx context.Context, method string, parameters JsonObject) (any, error) {
	if c.connection == nil {
		return nil, errors.New("PluginContext has no multiplexed JSON-RPC connection")
	}
	return c.connection.request(ctx, method, parameters)
}

func (c *PluginContext) LogWrite(ctx context.Context, message string, level string) (any, error) {
	if strings.TrimSpace(level) == "" {
		level = "Information"
	}
	return c.Invoke(ctx, "log.write", JsonObject{"level": level, "message": message})
}

type StdioJsonRpcPlugin struct {
	PluginID   string
	SDKName    string
	SDKVersion string

	// Optional local cap. Zero means use the value negotiated from Main.
	MaxConcurrentEvents int
	QueueCapacity       int
	RequestTimeout      time.Duration

	input       *bufio.Reader
	output      *bufio.Writer
	connection  *rpcConnection
	initialized atomic.Bool
	stopping    atomic.Bool
	eventGate   chan struct{}
	eventSlots  chan struct{}
	eventWG     sync.WaitGroup
}

func NewStdioJsonRpcPlugin(pluginID string, sdkName string, sdkVersion string) *StdioJsonRpcPlugin {
	if strings.TrimSpace(sdkName) == "" {
		sdkName = "iskypro-go-sdk-v2"
	}
	if strings.TrimSpace(sdkVersion) == "" {
		sdkVersion = DefaultSdkVersion
	}
	input := bufio.NewReader(os.Stdin)
	output := bufio.NewWriter(os.Stdout)
	return &StdioJsonRpcPlugin{
		PluginID:       pluginID,
		SDKName:        sdkName,
		SDKVersion:     sdkVersion,
		RequestTimeout: DefaultRequestTimeout,
		input:          input,
		output:         output,
		connection:     newRpcConnection(input, output),
	}
}

func (p *StdioJsonRpcPlugin) Run(ctx context.Context, onEvent EventHandler) error {
	if onEvent == nil {
		return errors.New("event handler is required")
	}

	incoming := make(chan JsonObject, 64)
	readErrors := make(chan error, 1)
	stopDone := make(chan struct{})
	go p.readLoop(ctx, incoming, readErrors)

	defer p.connection.closeWithError(io.EOF)
	for {
		select {
		case <-ctx.Done():
			p.connection.closeWithError(ctx.Err())
			return ctx.Err()
		case <-stopDone:
			return nil
		case err := <-readErrors:
			if errors.Is(err, io.EOF) {
				return nil
			}
			p.connection.closeWithError(err)
			return err
		case request := <-incoming:
			method, _ := request["method"].(string)
			switch method {
			case "iskypro.initialize":
				if err := p.handleInitialize(request["id"], asObject(request["params"])); err != nil {
					return err
				}
			case "plugin.stop", "shutdown":
				if p.stopping.CompareAndSwap(false, true) {
					go p.completeStop(request["id"], stopDone)
				}
			case "events.message":
				p.scheduleEvent(ctx, request["id"], asObject(request["params"]), onEvent)
			default:
				if request["id"] != nil {
					_ = p.writeError(request["id"], jsonRpcMethodNotFound, fmt.Sprintf("unknown method: %s", method))
				}
			}
		}
	}
}

func (p *StdioJsonRpcPlugin) readLoop(
	ctx context.Context,
	incoming chan<- JsonObject,
	readErrors chan<- error,
) {
	for {
		message, err := ReadFrame(p.input)
		if err != nil {
			readErrors <- err
			return
		}
		if p.connection.routeResponse(message) {
			continue
		}
		select {
		case incoming <- message:
		case <-ctx.Done():
			readErrors <- ctx.Err()
			return
		}
	}
}

func (p *StdioJsonRpcPlugin) handleInitialize(id any, parameters JsonObject) error {
	if id == nil {
		return errors.New("iskypro.initialize must be a JSON-RPC request")
	}
	if p.initialized.Load() {
		return p.writeError(id, jsonRpcPluginError, "Plugin SDK v2 is already initialized")
	}
	if !containsProtocolVersion(parameters["supportedProtocolVersions"], ProtocolVersion) {
		return p.writeError(id, jsonRpcInvalidParams, "Main does not support protocol version 2")
	}
	if pluginID := stringValue(parameters["pluginId"]); pluginID != p.PluginID {
		return p.writeError(id, jsonRpcInvalidParams, "initialize pluginId does not match the plugin")
	}
	if !strings.EqualFold(stringValue(parameters["encoding"]), "json") {
		return p.writeError(id, jsonRpcInvalidParams, "Plugin SDK v2 only supports json encoding")
	}

	p.connection.token = stringValue(parameters["token"])
	if p.connection.token == "" {
		p.connection.token = stringValue(parameters["runtimeToken"])
	}
	if p.connection.token == "" {
		return p.writeError(id, jsonRpcInvalidParams, "initialize token is required")
	}

	runtime := asObject(parameters["runtime"])
	mainConcurrency := positiveInt(runtime["maxConcurrentEvents"], 1)
	effectiveConcurrency := mainConcurrency
	if p.MaxConcurrentEvents > 0 && p.MaxConcurrentEvents < effectiveConcurrency {
		effectiveConcurrency = p.MaxConcurrentEvents
	}
	p.eventGate = make(chan struct{}, effectiveConcurrency)
	queueCapacity := positiveInt(runtime["queueCapacity"], 64)
	if p.QueueCapacity > 0 && p.QueueCapacity < queueCapacity {
		queueCapacity = p.QueueCapacity
	}
	p.eventSlots = make(chan struct{}, effectiveConcurrency+queueCapacity)
	timeoutMilliseconds := positiveInt(runtime["requestTimeoutMilliseconds"], int(p.RequestTimeout.Milliseconds()))
	p.connection.requestTimeout = time.Duration(timeoutMilliseconds) * time.Millisecond
	if p.connection.requestTimeout <= 0 || p.connection.requestTimeout > 5*time.Minute {
		p.connection.requestTimeout = DefaultRequestTimeout
	}

	p.initialized.Store(true)
	return p.writeSuccess(id, JsonObject{
		"protocolVersion": ProtocolVersion,
		"pluginId":        p.PluginID,
		"sdkName":         p.SDKName,
		"sdkVersion":      p.SDKVersion,
		"capabilities": []string{
			"events.message",
			"log.write",
			"messages.reply",
			"messages.send",
			"bidirectional-requests",
			"concurrent-events",
			"graceful-shutdown",
		},
		"encoding": "json",
	})
}

func (p *StdioJsonRpcPlugin) scheduleEvent(
	ctx context.Context,
	id any,
	event JsonObject,
	onEvent EventHandler,
) {
	if !p.initialized.Load() || p.eventGate == nil || p.eventSlots == nil {
		_ = p.writeError(id, jsonRpcInvalidRequest, "Plugin must be initialized before events")
		return
	}
	if p.stopping.Load() {
		_ = p.writeError(id, jsonRpcPluginError, "Plugin is stopping")
		return
	}
	select {
	case p.eventSlots <- struct{}{}:
	case <-ctx.Done():
		_ = p.writeError(id, jsonRpcPluginError, ctx.Err().Error())
		return
	default:
		if id != nil {
			_ = p.writeSuccess(id, JsonObject{
				"accepted": false,
				"eventId":  stringValue(event["eventId"]),
				"error":    "Plugin SDK v2 local event queue is full.",
			})
		}
		return
	}

	p.eventWG.Add(1)
	go func() {
		defer p.eventWG.Done()
		defer func() { <-p.eventSlots }()
		select {
		case p.eventGate <- struct{}{}:
			defer func() { <-p.eventGate }()
		case <-ctx.Done():
			_ = p.writeError(id, jsonRpcPluginError, ctx.Err().Error())
			return
		}

		pluginContext := newRuntimePluginContext(p.PluginID, p.connection)
		message := createMessageContext(event, pluginContext)
		ack, err := onEvent(ctx, message, pluginContext)
		if err != nil {
			ack = EventAck{Accepted: false, EventID: stringValue(event["eventId"]), Error: err.Error()}
		}
		if ack.EventID == "" {
			ack.EventID = stringValue(event["eventId"])
		}
		if id != nil {
			_ = p.writeSuccess(id, JsonObject{
				"accepted": ack.Accepted,
				"eventId":  ack.EventID,
				"error":    ack.Error,
			})
		}
	}()
}

func (p *StdioJsonRpcPlugin) completeStop(id any, done chan<- struct{}) {
	p.eventWG.Wait()
	if id != nil {
		_ = p.writeSuccess(id, JsonObject{"accepted": true})
	}
	close(done)
}

func (p *StdioJsonRpcPlugin) writeSuccess(id any, result any) error {
	return p.connection.write(JsonObject{
		"jsonrpc": JsonRpcVersion,
		"id":      id,
		"result":  result,
	})
}

func (p *StdioJsonRpcPlugin) writeError(id any, code int, message string) error {
	if id == nil {
		return nil
	}
	return p.connection.write(JsonObject{
		"jsonrpc": JsonRpcVersion,
		"id":      id,
		"error": JsonObject{
			"code":    code,
			"message": message,
		},
	})
}

func ReadFrame(input *bufio.Reader) (JsonObject, error) {
	var header bytes.Buffer
	for {
		value, err := input.ReadByte()
		if err != nil {
			if errors.Is(err, io.EOF) && header.Len() == 0 {
				return nil, io.EOF
			}
			return nil, err
		}

		header.WriteByte(value)
		if header.Len() > MaxHeaderLength {
			return nil, fmt.Errorf("stdio-jsonrpc header exceeds %d bytes", MaxHeaderLength)
		}
		if bytes.HasSuffix(header.Bytes(), []byte("\r\n\r\n")) {
			break
		}
	}

	contentLength := -1
	headerText := strings.TrimSuffix(header.String(), "\r\n\r\n")
	for _, line := range strings.Split(headerText, "\r\n") {
		if line == "" {
			continue
		}
		name, value, found := strings.Cut(line, ":")
		if !found || !strings.EqualFold(name, "Content-Length") {
			return nil, errors.New("stdio-jsonrpc only supports Content-Length headers")
		}
		if contentLength >= 0 {
			return nil, errors.New("stdio-jsonrpc Content-Length header must appear exactly once")
		}
		parsed, err := strconv.Atoi(strings.TrimSpace(value))
		if err != nil {
			return nil, fmt.Errorf("stdio-jsonrpc Content-Length is invalid: %w", err)
		}
		contentLength = parsed
	}

	if contentLength <= 0 {
		return nil, errors.New("stdio-jsonrpc frame is missing Content-Length")
	}
	if contentLength > MaxPayloadLength {
		return nil, fmt.Errorf("stdio-jsonrpc payload exceeds %d bytes", MaxPayloadLength)
	}

	payload := make([]byte, contentLength)
	if _, err := io.ReadFull(input, payload); err != nil {
		return nil, err
	}

	var message JsonObject
	if err := json.Unmarshal(payload, &message); err != nil {
		return nil, err
	}
	if err := validateMessage(message); err != nil {
		return nil, err
	}
	return message, nil
}

func WriteFrame(output *bufio.Writer, message JsonObject) error {
	if err := validateMessage(message); err != nil {
		return err
	}
	payload, err := json.Marshal(message)
	if err != nil {
		return err
	}
	if len(payload) <= 0 || len(payload) > MaxPayloadLength {
		return fmt.Errorf("invalid stdio-jsonrpc payload length: %d", len(payload))
	}
	if _, err := fmt.Fprintf(output, "Content-Length: %d\r\n\r\n", len(payload)); err != nil {
		return err
	}
	if _, err := output.Write(payload); err != nil {
		return err
	}
	return output.Flush()
}

func validateMessage(message JsonObject) error {
	if stringValue(message["jsonrpc"]) != JsonRpcVersion {
		return errors.New("JSON-RPC payload must declare jsonrpc 2.0")
	}
	method := stringValue(message["method"])
	if strings.TrimSpace(method) != "" {
		return nil
	}
	_, hasResult := message["result"]
	_, hasError := message["error"]
	_, hasID := message["id"]
	if !hasID || hasResult == hasError {
		return errors.New("JSON-RPC response must contain id and exactly one of result or error")
	}
	return nil
}

func parseJsonRpcError(value any) error {
	object := asObject(value)
	return &JsonRpcError{
		Code:    intValue(object["code"], jsonRpcPluginError),
		Message: stringValueOrJson(object["message"], value),
		Data:    object["data"],
	}
}

func createMessageContext(event JsonObject, pluginContext *PluginContext) *MessageContext {
	message := asObject(event["message"])
	sender := asObject(event["sender"])
	mentions := []UserRef{}
	if values, ok := message["mentions"].([]any); ok {
		for _, value := range values {
			mentions = append(mentions, userRefFromObject(asObject(value)))
		}
	}
	attachments := []any{}
	if values, ok := message["attachments"].([]any); ok {
		attachments = values
	}
	return &MessageContext{
		EventID:      stringValue(event["eventId"]),
		EventType:    stringValue(event["eventType"]),
		Timestamp:    event["timestamp"],
		Source:       stringValue(event["source"]),
		Bot:          asObject(event["bot"]),
		Conversation: asObject(event["conversation"]),
		Sender:       userRefFromObject(sender),
		ID:           stringValue(message["id"]),
		Text:         stringValue(message["content"]),
		Attachments:  attachments,
		Mentions:     mentions,
		RawPayload:   asObject(event["rawPayload"]),
		reference:    asObject(event["messageReference"]),
		context:      pluginContext,
	}
}

func userRefFromObject(value JsonObject) UserRef {
	return UserRef{
		Provider:     stringValue(value["provider"]),
		MentionID:    stringValue(value["mentionId"]),
		UserOpenID:   stringValue(value["userOpenId"]),
		MemberOpenID: stringValue(value["memberOpenId"]),
		UnionOpenID:  stringValue(value["unionOpenId"]),
		DisplayName:  stringValue(value["displayName"]),
	}
}

func asObject(value any) JsonObject {
	if value == nil {
		return JsonObject{}
	}
	if object, ok := value.(JsonObject); ok {
		return object
	}
	if object, ok := value.(map[string]any); ok {
		return JsonObject(object)
	}
	return JsonObject{}
}

func stringValue(value any) string {
	if text, ok := value.(string); ok {
		return text
	}
	return ""
}

func stringValueOrJson(value any, fallback any) string {
	if text := stringValue(value); text != "" {
		return text
	}
	payload, err := json.Marshal(fallback)
	if err != nil {
		return fmt.Sprint(fallback)
	}
	return string(payload)
}

func requestKey(value any) string {
	switch typed := value.(type) {
	case string:
		return typed
	case int:
		return strconv.Itoa(typed)
	case int64:
		return strconv.FormatInt(typed, 10)
	case float64:
		if math.Trunc(typed) == typed {
			return strconv.FormatInt(int64(typed), 10)
		}
		return strconv.FormatFloat(typed, 'g', -1, 64)
	default:
		return fmt.Sprint(value)
	}
}

func positiveInt(value any, fallback int) int {
	parsed := intValue(value, fallback)
	if parsed <= 0 {
		return fallback
	}
	return parsed
}

func intValue(value any, fallback int) int {
	switch typed := value.(type) {
	case int:
		return typed
	case int64:
		return int(typed)
	case float64:
		return int(typed)
	case json.Number:
		parsed, err := typed.Int64()
		if err == nil {
			return int(parsed)
		}
	}
	return fallback
}

func containsProtocolVersion(value any, expected int) bool {
	items, ok := value.([]any)
	if !ok {
		return false
	}
	for _, item := range items {
		if intValue(item, -1) == expected {
			return true
		}
	}
	return false
}
