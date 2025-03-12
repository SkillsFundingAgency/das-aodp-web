## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# AODP Web

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

## About

This service is used by Awarding Organisations to submit applications to be reviewed for funding.

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/Aodp) should be available either running locally or accessible in an Azure tenancy.
* The Inner API [das-aodp-api](https://github.com/SkillsFundingAgency/das-aodp-api) should be available either running locally or accessible in an Azure tenancy.

### Config
You can find the latest config file in [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-aodp-web/SFA.DAS.AODP.Web.json)


* If you are using Azure Storage Emulator for local development purpose, then In your Azure Storage Account, create a table called Configuration and Add the following

ParitionKey: LOCAL  
RowKey: SFA.DAS.AODP.Web_1.0  
Data:  
```json
{
  // content from latest config file
}
```

## Technologies
* .NetCore 8.0
* xUnit
* Moq
* Azure App Insights
* MediatR
* Redis


## License

Licensed under the [MIT license](LICENSE)