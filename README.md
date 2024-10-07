# Step Test Data Management Software
> **WARNING: This project is quite old and was created when I was still a beginner with C#, OOP, and the tools used in this application. It may not follow best practices and is a reflection of my early learning experience.**


## Introduction

The **Step Test Data Manager** is a desktop application designed to automate the process of collecting, analyzing, and storing data from Step Tests. The Step Test involves measuring a participant’s heart rate at various effort levels and calculating their aerobic capacity. This application simplifies the traditionally manual process, making it easier to track and analyze health data over time. The system supports multiple participants in one session, allowing for efficient data management in fitness centers or medical environments.


## Features & Functionality

- **User Registration**: Add new participants to the system by entering personal details such as name, date of birth, and email.
- **Step Test Management**: Conduct Step Tests by recording participants' heart rates, calculate aerobic capacity, and track their fitness rating.
- **Data Analysis**: View a graphical representation of test data with trend lines and regression analysis.
- **Session Management**: Run multiple tests in one session, switching between participants and storing test results.
- **Email Sharing**: Automatically send test results to participants via email.
- **Search and Filter**: Easily find participants or tests with a robust search and filter system.
- **Import/Export Participants**: Import participant data from files and manage a large number of records efficiently.


## Step Test Principle

The table below (found on the internet) illustrates the rating based on age and aerobic capacity during the Step Test. Given a gender, an age, and multiple recorded heart rates during moments of effort with a specific step height, the software can hence determine the Fitness Rating after calculating the aerobic capcaity.

<img src="https://www.whyiexercise.com/images/3-minute-step-test-chart-for-men.jpg" alt="Illustration of the principle of the Step Test" width="600"/>


## Project Structure

The project structure is relatively simple and may not follow the best practices, as this project was developed when I was still a beginner in C# and object-oriented programming. The project has been is structured as follows:

- **Step Test Manager Folder**: This folder contains the core files and resources of the project, including the main logic and settings.
  
  - **Solution File (.sln)**: The main solution file that can be opened in Visual Studio to view and edit the project codebase.
    
  - **Form1.cs**: Contains the main logic for the application, including:
    - **Session Management**: Logic for managing step test sessions, including test initialization, data entry, and completion.
    - **Database Handling**: Methods for adding, editing, and querying participant data from a database.
    - **Charting**: Visual representation of the test results with trend line analysis.
    - **Email Functionality**: Code to send emails with test results to participants.
  
  - **Database Files**: These include the `.xsc`, `.xsd`, `.xss`, and `.mdf` files that handle database schema and storage:
    - **.mdf**: Represents the actual database file for storing participant and test data.
    - **.xsc**, **.xsd**, **.xss**: Schema and dataset-related files used to define and structure the database.

  - **Resources Subfolder**: Contains assets such as icons and images that are used in the UI. 
    - **Images**: Images used within the UI, including icons and visual assets for the application.

  - **Bin/Debug Subfolder**: Contains the compiled executable (`Step Test Manager.exe`) that can be run to start the application.

  - **Properties Subfolder**: This subfolder stores modifiable application settings ('Settings.settings') that can be adjusted through the software. These settings are saved and persisted across sessions.
  

## Technologies Used

- **C# (WinForms)**: The core programming language used to build the desktop application.
- **Entity Framework**: ORM used for interacting with the database.
- **SQL Server**: Database system used to store participant and test data.
- **Windows Forms Data Visualization**: For rendering test results and charts.
- **SMTP (Gmail)**: For sending test results via email to participants.

  
## Prerequisites for Setup & Installation

- Visual Studio with .NET Framework installed.
- SQL Server or any other database for data storage.
- Email credentials to enable the emailing functionality (Gmail configuration is used in this project).

## Application Walkthrough & Screenshots
Below are some screenshots of all pages that show how the software works and looks. The UI remains very basic with improveable aesthetics.  All images are located in the `App Demo Images` folder of this repository.

#### Home
This is the home screen where you can see the list of all registered people. It includes buttons  advanced search inputs on the right, navigation buttons at the top to access all other features, and, for each person, buttons to choose and view their tests, or take a new test for them. But one can also choose to create a new test before choosing a person. Clicking a person will show their information on the right instead of the search parameters, and enable the user to edit it.

<img src="App Demo Images/Home.png" alt="Home Screen" width="600px">

---

#### Register a new Person
This form is used to create a new person in the database, allowing them to take Step Tests.

<img src="App Demo Images/Person Registration.png" alt="Person Registration Form" width="600px">

---

#### Take a New Test
This small form is used when creating a new test. You can input essential information for the new test session.

<img src="App Demo Images/Create New Test.png" alt="Create New Test Form" width="600px">

---

#### Choose a Test Taker
In this window, you can select a person from the list to take a test or complete a form to create a new person if they are not already registered.

<img src="App Demo Images/Choose Test Taker.png" alt="Choose Test Taker Window" width="600px">

---

#### Test Performance Input
This screen allows you to input the heart rate for each moment of effort, with a timer to start, pause, and restart for ease during the test.

<img src="App Demo Images/Test Input.png" alt="Test Input Screen" width="600px">

---

#### View Test Overview and Results
This page displays all pre-test information, test input fields, and person details, along with the test results. You can also add comments, save the test, or return to the home screen from here.

<img src="App Demo Images/Test Input and Results.png" alt="Test Input and Results Screen" width="600px">

---

### View Analytics

The Analytics page gives a global view of all the tests taken (this feature is currently unfinished).

<img src="App Demo Images/Analytics.png" alt="Analytics Page" width="600px">

---

### Change Settings

This section allows you to modify settings for future tests, such as step height, tester's initials, and whether to send test results by email.

<img src="App Demo Images/Settings.png" alt="Settings Page" width="600px">


## Possible Enhancements
- Export Functionality: Allow exporting test results in PDF or CSV format rather than text.
- Additional Analytics: Add more visualizations and data comparisons based on user performance.
- Enhanced User Management: Implement role-based access and more customization options for users.
