# Department for Education (DfE) Frontend

DfE frontend contains the code you need to design DfE Branded services on top of the GOVUK Design System.

## Installation of the DfE Frontend package

`npm i dfe-frontend`

## Using this package

You can find guidance for using this package in the [DfE Design manual](https://design.education.gov.uk/design-system).

## Help and support

Contact the [DfE DesignOps team](https://design.education.gov.uk/design-ops).

## Version history

### V2.0.1 

- Updated readme

### V2.0.0

[Read the guidance](https://design.education.gov.uk/design-system/dfe-frontend/updating-from-alpha) for updating from dfe-frontend-alpha to dfe-frontend

To upgdate your code from `dfe-frontend-alpha` to `dfe-frontend`, remove the `dfe-frontend-alpha` package  and install dfe-frontend using npm i dfe-frontend.

You will then need to replace `dfe-frontend-alpha` with `dfe-frontend` in all references and paths for example to node_modules.

- Supercedes dfe-frontend-alpha package which will remain at 1.0.1 and should not be used going forward.
- Introduces card component
- Fixes and improvement to the header component
