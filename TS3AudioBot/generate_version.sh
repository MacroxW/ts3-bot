#!/bin/bash
# Usage: generate_version.sh <output_file> <major> <minor> <build> <configuration>
cat > "$1" << EOF
[assembly: System.Reflection.AssemblyVersion("$2.$3.$4")]
[assembly: System.Reflection.AssemblyFileVersion("$2.$3.$4")]
[assembly: System.Reflection.AssemblyInformationalVersion("$2.$3.$4")]
namespace TS3AudioBot.Environment {
    partial class BuildData {
        partial void GetDataInternal() {
            this.Version = "$2.$3.$4";
            this.Branch = "master";
            this.CommitSha = "unknown";
            this.BuildConfiguration = "$5";
        }
    }
}
EOF