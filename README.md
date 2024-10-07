# Step Test Data Management Software

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

- **Form1.cs**: Contains the main logic for the application including the UI control, data handling, and event processing. It includes.
  - **Session Management**: Logic for managing step test sessions, including test initialization, data entry, and completion.
  - **Database Handling**: Methods for adding, editing, and querying participant data from a database.
  - **Charting**: Visual representation of the test results with trend line analysis.
  - **Email Functionality**: Code to send emails with test results to participants.
  
- **Database Layer**: The `DBEntities` handles the data storage for participants and tests, ensuring persistence between sessions.
  
- **UI Components**: Several form components (`TabControl`, `DataGridView`, etc.) handle the navigation between adding participants, managing sessions, and viewing test data.

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

## Screenshots & Illustrations

## Possible Enhancements
- Export Functionality: Allow exporting test results in PDF or CSV format rather than text.
- Additional Analytics: Add more visualizations and data comparisons based on user performance.
- Enhanced User Management: Implement role-based access and more customization options for users.
