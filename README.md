# LascheApp

Windows desktop application for verifying transport and tension lugs. It performs
geometry, material, shackle, pin, bearing and angled-pull checks and presents the
governing result in a structured summary.

## Features

- Transport lug and tension lug verification
- Configurable material and shackle databases
- English and German interface and reports
- Geometry guidance and preliminary sizing
- Printable and editable RTF reports
- Non-blocking version checking and privacy-conscious usage telemetry

## Running the application

Use `Setup_LascheApp.exe` for a normal Windows installation. A portable x64 ZIP
package is also available; extract it completely before starting `LascheApp.exe`.
The published application is self-contained and does not require a separate .NET
installation.

## Configuration

Deployment settings are read from `settings.json` beside the executable. This file
is excluded from Git because it can contain environment-specific endpoints. Use
`settings.template.json` as the starting point.

## Development

The project targets .NET 8 and Windows Forms:

```powershell
dotnet build LascheApp.sln
```

Release publishing is configured in `Properties/PublishProfiles/WinX64.pubxml` and
the Inno Setup installer definition is in `Installer/LascheApp.iss`.
