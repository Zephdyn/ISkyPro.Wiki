package main

import "testing"

func TestManifestPlatform(t *testing.T) {
	if got := manifestPlatform("darwin"); got != "osx" {
		t.Fatalf("manifestPlatform(darwin) = %q, want osx", got)
	}
	if got := manifestPlatform("linux"); got != "linux" {
		t.Fatalf("manifestPlatform(linux) = %q, want linux", got)
	}
}

func TestManifestArchitecture(t *testing.T) {
	if got := manifestArchitecture("amd64"); got != "x64" {
		t.Fatalf("manifestArchitecture(amd64) = %q, want x64", got)
	}
	if got := manifestArchitecture("386"); got != "x86" {
		t.Fatalf("manifestArchitecture(386) = %q, want x86", got)
	}
}
