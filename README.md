# EBrief

## Table of Contents

- [Description](#description)
- [Installation](#installation)
- [Usage](#usage)
- [Questions](#questions)
- [Licence](#licence)

## Description

EBrief is a system for prosecutors to efficiently prepare for and conduct court lists. It enables prosecutors to download court files to their local machine prepare for upcoming court hearings, and to have rapid access to information about court files during court hearings.

EBrief was designed to address inefficiencies in the current system which was gated by poor network conditions.

The front end of the application is a WPF application shell for a webview built with Blazor Web Assembly and styled with Tailwind CSS. Data is retrieved from the organisation's database and stored locally on the user's machine with Sqlite.

Court lists can be exported to a custom file type and shared with other users who can then import the file into their own instance of the application.

This repo does not include the installer for the application as features are still being developed.

## Installation

This application is only designed to work on Windows machines.

Clone the repo to your local machine:

```bash
  git clone https://github.com/craigrobertsdev/Social-Network-API.git
```

<br>

You will need the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.

The application depends on dummy data being provided by a server which can be found [here](https://github.com/craigrobertsdev/DocumentServer).

## Usage

1. Start the document server:
   In your terminal, navigate to the folder containing the Document Server and run the following command:

```bash
dotnet run -lp https
```

  <br>

2. Start the application:
   Run the application from your text editor of choice, or from the terminal using the following command:

```bash
dotnet run
```

<br>

3. The first time you launch the application, you will need to generate a court list by clicking on the `Add New Court List` button. As the landscape lists used by this application are not public documents, you will need to click the `Enter manually?` button. <br><br>Fill in the information in the dropdowns (it is all test data so you can choose any option) and provide a list of unique numbers separated by spaces or line breaks. A sample list is stored [here](https://github.com/craigrobertsdev/EBrief/blob/main/sample-case-numbers.txt).

![Home screen](https://github.com/craigrobertsdev/EBrief/blob/main/Images/Home%20Screen.png)

![Information screen](https://github.com/craigrobertsdev/EBrief/blob/main/Images/Information%20screen.png)

## Questions

If you would like to discuss this project further, please contact me at craig.roberts11@outlook.com, or via [LinkedIn](https://www.linkedin.com/in/craig-roberts-9ba409243/).

## Licence

This application is subject to the Creative Commons Licence v1.0 Universal.
