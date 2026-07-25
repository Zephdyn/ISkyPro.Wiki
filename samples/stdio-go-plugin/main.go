package main

import (
	"context"
	"fmt"
	"os"
	"strings"

	"github.com/Zephdyn/ISkyPro.Wiki/sdk/go/iskypro/v2"
)

const pluginID = "top.iskypro.sample.go-stdio"

func main() {
	if !hasArg("--iskypro-stdio") {
		fmt.Fprintln(os.Stderr, "This plugin is meant to be run by ISkyPro with --iskypro-stdio.")
		os.Exit(2)
	}

	plugin := iskypro.NewStdioJsonRpcPlugin(pluginID, "iskypro-go-sdk-v2", iskypro.DefaultSdkVersion)
	if err := plugin.Run(context.Background(), onEvent); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
}

func onEvent(ctx context.Context, event iskypro.JsonObject, pluginContext *iskypro.PluginContext) (iskypro.EventAck, error) {
	eventID := valueString(event["eventId"])
	message := objectValue(event["message"])
	reference := objectValue(event["messageReference"])
	content := valueString(message["content"])

	fmt.Fprintf(os.Stderr, "go sample received %s\n", eventID)
	if _, err := pluginContext.LogWrite(ctx, "go sample handled "+eventID, "Information"); err != nil {
		return iskypro.EventAck{Accepted: false, EventID: eventID, Error: err.Error()}, nil
	}
	if content != "" {
		if _, err := pluginContext.ReplyText(ctx, reference, "go echo: "+content); err != nil {
			return iskypro.EventAck{Accepted: false, EventID: eventID, Error: err.Error()}, nil
		}
	}

	return iskypro.EventAck{Accepted: true, EventID: eventID}, nil
}

func hasArg(expected string) bool {
	for _, arg := range os.Args[1:] {
		if arg == expected {
			return true
		}
	}
	return false
}

func objectValue(value any) iskypro.JsonObject {
	if object, ok := value.(map[string]any); ok {
		return iskypro.JsonObject(object)
	}
	if object, ok := value.(iskypro.JsonObject); ok {
		return object
	}
	return iskypro.JsonObject{}
}

func valueString(value any) string {
	if value == nil {
		return ""
	}
	if text, ok := value.(string); ok {
		return text
	}
	text := strings.TrimSpace(fmt.Sprint(value))
	if text == "<nil>" {
		return ""
	}
	return text
}
