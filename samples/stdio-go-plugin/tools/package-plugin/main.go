package main

import (
	"archive/zip"
	"encoding/json"
	"flag"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	goruntime "runtime"
	"strings"
)

func main() {
	goos := flag.String("goos", goruntime.GOOS, "target GOOS")
	goarch := flag.String("goarch", goruntime.GOARCH, "target GOARCH")
	flag.Parse()

	root := sampleRoot()
	manifest := readManifest(filepath.Join(root, "manifest.json"))
	pluginID, _ := manifest["pluginId"].(string)
	if strings.TrimSpace(pluginID) == "" {
		fatalf("manifest.json must contain pluginId")
	}

	temporary, err := os.MkdirTemp("", "iskypro-go-plugin-")
	if err != nil {
		fatalf("create temporary directory: %v", err)
	}
	defer os.RemoveAll(temporary)

	binaryName := "stdio-go-plugin"
	if *goos == "windows" {
		binaryName += ".exe"
	}
	binaryPath := filepath.Join(temporary, binaryName)
	command := exec.Command("go", "build", "-buildvcs=false", "-trimpath", "-o", binaryPath, ".")
	command.Dir = root
	command.Env = append(os.Environ(), "GOOS="+*goos, "GOARCH="+*goarch)
	command.Stdout = os.Stdout
	command.Stderr = os.Stderr
	if err := command.Run(); err != nil {
		fatalf("go build failed: %v", err)
	}

	stdio := manifest["transport"].(map[string]any)["stdio"].(map[string]any)
	stdio["command"] = "./" + binaryName
	stdio["args"] = []string{"--iskypro-stdio"}
	manifest["supportedPlatforms"] = []map[string]any{
		{"platform": manifestPlatform(*goos), "architectures": []string{manifestArchitecture(*goarch)}},
	}
	manifestBytes, err := json.MarshalIndent(manifest, "", "  ")
	if err != nil {
		fatalf("serialize release manifest: %v", err)
	}
	manifestBytes = append(manifestBytes, '\n')

	artifacts := filepath.Join(root, "artifacts")
	if err := os.MkdirAll(artifacts, 0o755); err != nil {
		fatalf("create artifacts directory: %v", err)
	}
	archivePath := filepath.Join(artifacts, fmt.Sprintf("%s-%s-%s.zip", pluginID, *goos, *goarch))
	archive, err := os.Create(archivePath)
	if err != nil {
		fatalf("create zip: %v", err)
	}
	zipWriter := zip.NewWriter(archive)
	addBytes(zipWriter, "manifest.json", manifestBytes, 0o644)
	addFile(zipWriter, binaryName, binaryPath, 0o755)
	if _, err := os.Stat(filepath.Join(root, "README.md")); err == nil {
		addFile(zipWriter, "README.md", filepath.Join(root, "README.md"), 0o644)
	}
	if err := zipWriter.Close(); err != nil {
		fatalf("close zip: %v", err)
	}
	if err := archive.Close(); err != nil {
		fatalf("close archive: %v", err)
	}

	fmt.Printf("ISkyPro plugin package: %s\n", archivePath)
}

func sampleRoot() string {
	_, source, _, ok := goruntime.Caller(0)
	if !ok {
		fatalf("cannot locate package source")
	}
	return filepath.Clean(filepath.Join(filepath.Dir(source), "..", ".."))
}

func readManifest(path string) map[string]any {
	data, err := os.ReadFile(path)
	if err != nil {
		fatalf("read manifest: %v", err)
	}
	var manifest map[string]any
	if err := json.Unmarshal(data, &manifest); err != nil {
		fatalf("parse manifest: %v", err)
	}
	return manifest
}

func manifestArchitecture(goarch string) string {
	switch goarch {
	case "amd64":
		return "x64"
	case "386":
		return "x86"
	default:
		return goarch
	}
}

func manifestPlatform(goos string) string {
	if goos == "darwin" {
		return "osx"
	}
	return goos
}

func addFile(writer *zip.Writer, name, path string, mode os.FileMode) {
	file, err := os.Open(path)
	if err != nil {
		fatalf("open %s: %v", path, err)
	}
	defer file.Close()
	header := &zip.FileHeader{Name: name, Method: zip.Deflate}
	header.SetMode(mode)
	entry, err := writer.CreateHeader(header)
	if err != nil {
		fatalf("create zip entry %s: %v", name, err)
	}
	if _, err := io.Copy(entry, file); err != nil {
		fatalf("write zip entry %s: %v", name, err)
	}
}

func addBytes(writer *zip.Writer, name string, data []byte, mode os.FileMode) {
	header := &zip.FileHeader{Name: name, Method: zip.Deflate}
	header.SetMode(mode)
	entry, err := writer.CreateHeader(header)
	if err != nil {
		fatalf("create zip entry %s: %v", name, err)
	}
	if _, err := entry.Write(data); err != nil {
		fatalf("write zip entry %s: %v", name, err)
	}
}

func fatalf(format string, args ...any) {
	fmt.Fprintf(os.Stderr, format+"\n", args...)
	os.Exit(1)
}
