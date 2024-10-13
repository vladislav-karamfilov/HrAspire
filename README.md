# HrAspire

HrAspire is a simple but resilient, observable, cloud ready distributed HR system. It is built with [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) - Microsoft's new framework for building observable, production ready, distributed applications.

HrAspire is a project that I built in my spare time with a couple of key goals in mind:
- learn [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- explore and deepen my knowledge of technologies like [PostgreSQL](https://www.postgresql.org/), [RabbitMQ](https://www.rabbitmq.com/), [Garnet](https://microsoft.github.io/garnet/docs), [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview), [gRPC](https://grpc.io/), which I know about but haven't used in projects at work or in other personal projects
- explore and deepen my knowledge of libraries like [MassTransit](https://masstransit.io/), [FluentValidation](https://docs.fluentvalidation.net/), [Mapperly](https://mapperly.riok.app/), which I know about but haven't used in projects at work or in other personal projects
- practice my software design skills
- have fun architecturing and coding!

## Functionalities
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
HR managers have all the capabilites of an employee plus additional HR-specific management features:
- view and create/update/delete employees
- access details of all employees
- view and create/update/delete documents for all employees
- view and approve/reject salary (increase) requests for all employees
- view vacation requests for all employees
