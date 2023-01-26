# UpdatePasswordProtection

[Proof of Concept]
This is an AD FS plugin that utilizes the Risk Assesement framework to provide capabilities of protection against password spray and this kind of attacks on the Update password endpoint.

## Overview

By deafult the **/adfs/portal/updatepassword** endpoint is disabled on newly installed AD FS (Windows Server 2016 and above) farm. It's main goal is to provide password change capabilities for the users. To update their passwords users need to know the current one as this endpoint enforces no MFA whatsoever. The smart lockout capabilities were confirmed to not protecting this endpoint against password guessing kind of things therefore keeping it enabled might be searious risk to consider for companies. Windows Server 2019 comes with functionallity called  [Risk Assesment Framework](https://learn.microsoft.com/en-us/windows-server/identity/ad-fs/development/ad-fs-risk-assessment-model). This example uses SQLite database to store users that failed to provide correct password more than 3 times.

## Disclaimer

This is POC code. It's provided "AS IS". Definietly not ready to be used on Production environment

## Installation

This plugin capability has been introduced into Windows Server 2019 threfore this is the lowest system version that supports this plugin.

## Dependencies

- SQLite

## Usage

This plugin uses database and configuration file to operate. Currently database must be located under `C:\dev` and config file stopped working but might be stored anywhere as it's configured by `Register-AdfsThreatDetectionModule` powershell cmdlet.

## Features

- [x] Lock the user when the count of failed password retries is 3
- [x] Checking if request is from TOR network (not yet tested)
- [x] Event log instead of a file

## todo

- [ ] Code Cleanup
- [ ] Add powershell module to unlock / manage the plugin
- [ ] Test performance of the logic and impact on adfs service

## Further reading

[Risk Assesment Framework](https://learn.microsoft.com/en-us/windows-server/identity/ad-fs/development/ad-fs-risk-assessment-model)

[riskyIP plugin by Microsoft](https://github.com/microsoft/adfs-sample-RiskAssessmentModel-RiskyIPBlock)

[riskyUsers plugin by Microsoft](https://github.com/microsoft/adfs-sample-block-user-on-adfs-marked-risky-by-AzureAD-IdentityProtection)

## Contributing

Please open an issue first to discuss what you would like to change.

## License

GNU GENERAL PUBLIC LICENSE
