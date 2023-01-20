# UpdatePasswordProtection

[Proof of Concept]
This is an AD FS plugin that utilizes the Risk Assesement framework to provide capabilities of protection against password spray and this kind of attacks on the Update password endpoint.

## Overview

By deafult the /adfs/portal/updatepassword endpoint is disabled on newly installed AD FS (Windows Server 2012R2 and above). The main goal is to provide password change capabilities for the users while the only factor needed to successfully change password is knowing the current one. This endpoint enforces no MFA whatsoever. The smart lockout capabilities were confirmed to not protecting this endpoint against password guessing kind of things therefore keeping it enabled might be searious risk to consider for companies. Windows Server 2019 comes with functionallity called  [Risk Assesment Framework](https://learn.microsoft.com/en-us/windows-server/identity/ad-fs/development/ad-fs-risk-assessment-model). This example uses SQLite database to store users that failed to provide correct password more than 3 times.

## Disclaimer

This is POC code. It's provided "AS IS". Definietly not ready to be used on Production environment

## Installation

This plugin capability has been introduced into Windows Server 2019 threfore this is the lowest system version that supports this plugin.

## Dependencies

- SQLite
- Newtonsoft.Json

## Usage

## Features

- [x] Lock the user when the count of failed password retries is 3

## todo

- [ ] Code Cleanup
- [ ] Add powershell module to unlock / manage the plugin
- [ ] event log instead of a file
- [ ] Test performance of the logic and impact on adfs service

## Further reading

[Risk Assesment Framework](https://learn.microsoft.com/en-us/windows-server/identity/ad-fs/development/ad-fs-risk-assessment-model)

[riskyIP plugin by Microsoft](https://github.com/microsoft/adfs-sample-RiskAssessmentModel-RiskyIPBlock)

[riskyUsers plugin by Microsoft](https://github.com/microsoft/adfs-sample-block-user-on-adfs-marked-risky-by-AzureAD-IdentityProtection)

## Contributing

Please open an issue first to discuss what you would like to change.

## License

GNU GENERAL PUBLIC LICENSE
