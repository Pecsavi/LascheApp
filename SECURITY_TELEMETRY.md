# Security, logging and telemetry

`settings.json` is deployment-specific and intentionally excluded from Git. Copy
`settings.template.json` beside the executable, rename it to `settings.json`, then
set the real endpoints outside version control. Never commit production URLs,
tokens, certificate thumbprints, passwords or signing material.

The local application log is stored under `%LOCALAPPDATA%\LascheApp\Logs`. Log
messages must not contain usernames, project/customer data, report contents,
document names or paths.

When telemetry is explicitly enabled, the application sends only these fields:

- program identifier;
- event name (`application_started` or `verification_completed`);
- machine name;
- timestamp;
- application version.

The machine name can be personal data. Before enabling telemetry, publish a
privacy notice that identifies the controller, purpose, legal basis, recipients,
retention period and data-subject rights. Protect server logs and define automatic
deletion according to that retention period.

Telemetry and version-check failures are non-fatal and never display an error
dialog. `verification_completed` is emitted only after a report was successfully
generated.

## Release signing

Build the final Release artifacts first, then Authenticode-sign and RFC 3161
timestamp the EXE, relevant DLLs and the completed installer. Keep the certificate
thumbprint in protected release configuration, not in this repository. Use SHA-256
for both file and timestamp digests, verify every artifact with `signtool verify
/pa /all /v`, and publish only the verified files without modifying them.
