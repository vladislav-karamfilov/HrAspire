# HrAspire

HrAspire is a simple but resilient, observable, cloud ready distributed HR system. It is built with Microsoft's new framework for building observable, production ready, distributed applications [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/).

# Functionalities
HrAspire is a simple HR system designed to streamline employee management. The system offers role-based functionality for Employees, Managers and HR Managers, enabling efficient handling of common HR tasks. 

### Employee functionalities
All app users are employees including users in Manager and HR Manager roles. Employees can:
- log in
- view own employee details - email, full name, date of birth, position, department, manager, salary, used paid vacation days, etc.
- view and create/update/delete vacation requests for paid or unpaid leaves
- change password
- log out

### Manager functionalities
Managers have all the capabilities of an employee plus additional management features:
- view all managed employees
- access details of all managed employees
- view and create/update/delete documents for managed employees
- view and create/update/delete salary (increase) requests for managed employees
- view and approve/reject vacation requests for managed employees

### HR Manager functionalities
HR managers have all the capabilites of an employee plus additional HR specific management features:
- view and create/update/delete employees
- access details of all employees
- view and create/update/delete documents for all employees
- view and approve/reject salary (increase) requests for all employees
- view vacation requests for all employees

# Architecture
TODO

# Goals
TODO