# TC421
Reverse engineering for TC421 (TimeControl)

## TC420 with WiFi

### Description:

Reverse engineering for TC421 (TimeControl) for controlling your TC421 led controller remotely

### Usage:
### `TC421 [command] [options]`

### Options:
 	?, -h, --help          Show help and usage information
	--generate <generate>  Generate empty model file (as template). []
	--upload <upload>      Upload model to TC421 controller.
	--version              Show version information

### Commands:
	sync  Synchronize time.
	mac   Get MAC from device.

## How to run on Windows / Linux / Raspberry Pi:
### Windows:
`TC421.exe upload profile.json`
### Linux:
`dotnet TC421.dll upload profile.json`

## How to install dotnet on Linux / Raspberry Pi
	curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel STS
	echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
	echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
	source ~/.bashrc
	dotnet --version
