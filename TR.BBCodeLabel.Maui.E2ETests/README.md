# TR.BBCodeLabel.Maui E2E Tests

Cross-platform Appium-based E2E tests for the sample MAUI app. Targets
Android, iOS, and Windows.

## Required environment variables

| Variable               | Description                                                           |
|------------------------|-----------------------------------------------------------------------|
| `APPIUM_PLATFORM`      | `android`, `ios`, or `windows`                                        |
| `APPIUM_SERVER_URL`    | Optional. Defaults to `http://127.0.0.1:4723/`                        |
| `APP_PATH`             | Path to the built artifact (`.apk`, `.app`, or Windows `.exe`/AppId). |
| `APP_PACKAGE`          | Android only. Used when `APP_PATH` is not provided.                   |
| `APP_ACTIVITY`         | Android only. Optional launch activity.                               |
| `APPIUM_DEVICE_NAME`   | Optional override for device/simulator name.                          |
| `APPIUM_PLATFORM_VERSION` | Optional OS version pin.                                            |

## Local example (Android)

```sh
appium --base-path / &
export APPIUM_PLATFORM=android
export APP_PATH=/abs/path/to/com.techotter.bbcodelabel.sample-Signed.apk
dotnet test
```

## Local example (Windows)

```powershell
# Start WinAppDriver (or appium with appium-windows-driver) on port 4723
$env:APPIUM_PLATFORM = "windows"
$env:APP_PATH = "C:\path\to\TR.BBCodeLabel.Maui.Sample.exe"
dotnet test
```

## Local example (iOS, macOS host only)

```sh
appium --base-path / &
export APPIUM_PLATFORM=ios
export APP_PATH=/abs/path/to/TR.BBCodeLabel.Maui.Sample.app
export APPIUM_DEVICE_NAME="iPhone 15"
export APPIUM_PLATFORM_VERSION="17.5"
dotnet test
```
