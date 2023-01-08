using BusinessLogicLayer;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace Employee_Leave_Booking_System
{
    internal class Program
    {
        static void Main(string[] args)
        { 
            int mainMenuChoice = 0;
            LeaveSystemEntities context = new LeaveSystemEntities();
            EmployeeRepository er = new EmployeeRepository(context);
            EmployeeTypeRepository et = new EmployeeTypeRepository(context);
            LeaveApplicationRepository lar = new LeaveApplicationRepository(context);
            LeaveTypeRepository ltr = new LeaveTypeRepository(context);

            do
            {
                Console.Clear();
                Console.WriteLine("1. Add Employee");
                Console.WriteLine("2. Book Leave");
                Console.WriteLine("3. List Employees");
                Console.WriteLine("4. List Leave Applications for Employee");
                Console.WriteLine("5. Authroize Leave");
                Console.WriteLine("6. Reset Password");
                Console.WriteLine("7. Quit");
                mainMenuChoice = Convert.ToInt32(Console.ReadLine());

                switch(mainMenuChoice)
                {
                    case 1:
                        try
                        {

                            Employee e = new Employee();
                            Console.WriteLine("Enter Name: ");
                            e.Name = Console.ReadLine();
                            Console.WriteLine("Enter Surname: ");
                            e.Surname = Console.ReadLine();
                            Console.WriteLine("Enter Ni Number: (Max 10 Characters)");
                            e.Ni_number = Console.ReadLine();

                            Console.WriteLine("Enter Address Number: ");
                            string number = Console.ReadLine();
                            Console.WriteLine("Enter Address Street Name: ");
                            string streetName = Console.ReadLine();
                            Console.WriteLine("Enter Address Locality: ");
                            string locality = Console.ReadLine();
                            e.Address = number+", "+streetName+", "+locality;

                            Console.WriteLine("Enter Email: ");
                            e.Email = Console.ReadLine();

                            Console.WriteLine("Employee Types: ");
                            foreach (var x in et.GetEmployeeTypes() )
                            {
                                Console.WriteLine($"{x.ID} - {x.Title}");
                            }
                            Console.WriteLine("Enter EmployeeType: ");
                            e.EmployeeTypeFk = Convert.ToInt32(Console.ReadLine());
                            int count = er.GetEmployees().Count();
                            e.ID = count++;
                            er.AddEmployee(e);
                            Console.WriteLine("Employee added!");
                            Console.WriteLine("Press a key to continue.");
                            Console.ReadKey();
                        }
                        catch (FormatException ex) // only exceptions related when there's normally a conversion
                        {
                            Console.WriteLine("Invalid format inputted. Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("There was an error while communicating with the database. It might be that you have input incorrect values. Check again");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Some inputs may be invalid, Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        break;

                    case 2:
                        Console.Clear();
                        try { 
                        LeaveApplication la = new LeaveApplication();
                        Console.WriteLine("Enter Employee Id: ");
                        la.EmployeeIdFK= Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Leave Type: ");      
                        foreach (var x in ltr.GetLeaveTypes())
                        {
                            Console.WriteLine($"{x.ID} - {x.Title}");
                        }
                        Console.WriteLine("Enter number of leave type: ");
                        la.LeaveTypeFK = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Enter number of hours to be booked: ");
                        la.HoursBooked = Convert.ToInt32(Console.ReadLine());

                        //Validation to be inputted so that it is check the dateTo entered is not less than the dateFrom
                        Console.WriteLine("Enter Date from: ");
                        Console.WriteLine("Input day");
                        int day = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Input month");
                        int month = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Input year");
                        int year = Convert.ToInt32(Console.ReadLine());

                        la.DateFrom = new DateTime(year, month, day);
                        
                        Console.WriteLine("Enter Date to: ");
                        Console.WriteLine("Input day");
                        int day1 = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Input month");
                        int month1 = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Input year");
                        int year1 = Convert.ToInt32(Console.ReadLine());
                        la.DateTo = new DateTime(year1, month1, day1);

                        la.Status = 1;
                       
                        Console.WriteLine("Further comments: (Can be left empty)");
                        la.FurtherComments = Console.ReadLine();



                            //System.NullReferenceException: 'Object reference not set to an instance of an object.'
                            if (la.DateFrom < la.DateTo) {
                                if (lar.GetLeaveApplications(la.EmployeeIdFK, la.LeaveTypeFK).Count() == 0)
                                {
                                    lar.AddLeaveApplication(la);
                                }
                                else
                                {

                                    decimal sum = lar.GetLeaveApplicationsHoursSum(la.EmployeeIdFK, la.LeaveTypeFK);
                                    decimal hoursEntitled = ltr.GetHoursEntitled(la.LeaveTypeFK);
                                    decimal hoursLeft = Decimal.Subtract(hoursEntitled, sum);

                                    if (la.HoursBooked < hoursLeft)
                                    {
                                        lar.AddLeaveApplication(la);
                                        Console.WriteLine("Application submitted successfully!");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"{hoursLeft}hours left");
                                        Console.WriteLine("Not enough hours left to book requested Leave");
                                    }

                                }
                            } else
                            {
                                Console.WriteLine("Date to must be larger than Date from");
                            }
                        }
                        catch (FormatException ex) // only exceptions related when there's normally a conversion
                        {
                            Console.WriteLine("Invalid format inputted. Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("There was an error while communicating with the database. It might be that you have input incorrect values. Check again");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Some inputs may be invalid, Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }

                        Console.WriteLine("Press a key to continue.");
                        Console.ReadKey();
                        break;

                    case 3:
                        //var list = er.GetEmployees();
                        try { 
                        var list = er.ListEmployees();
                        Employee e1 = new Employee();
                        Authorizer a = new Authorizer();

                        if(list.Count() == 0)
                        {
                            Console.WriteLine("No Employees in List!");
                        } else
                        {
                            foreach (var employee in list)
                            {
                                Console.WriteLine(employee.ToString());
                                
                                Console.WriteLine("---------------------------");
                                Console.WriteLine("");

                            }
                        }
                        
                        Console.WriteLine("Press a key to continue.");
                        Console.ReadKey();
                        }
                        catch (FormatException ex) // only exceptions related when there's normally a conversion
                        {
                            Console.WriteLine("Invalid format inputted. Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("There was an error while communicating with the database. It might be that you have input incorrect values. Check again");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Some inputs may be invalid, Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        break;

                    case 4:
                        try { 
                        Console.WriteLine("Enter Id of Employee to see leave applications");
                        int choice4 = Convert.ToInt32(Console.ReadLine());
                        Console.Clear();

                        var listOfApplications = lar.GetListLeaveApplications(choice4);
                        if(listOfApplications.Count() > 0)
                        {
                            Console.WriteLine("List of Leave applications:");
                            foreach( var application in listOfApplications)
                            {
                                Console.WriteLine($"Id: {application.Id}");
                                Console.WriteLine($"Employee Id: {application.EmployeeIdFk}");
                                Console.WriteLine($"Leave Type: {application.LeaveType.Title}");
                                Console.WriteLine($"Hours Booked: {application.HoursBooked}");
                                Console.WriteLine($"Date From: {application.DateFrom}");
                                Console.WriteLine($"Date To: {application.DateTo}");
                                switch (application.Status)
                                {
                                    case 1:
                                        Console.WriteLine("Status: Pending");
                                        break;
                                    case 2:
                                        Console.WriteLine("Status: Authorized");
                                        break;
                                    case 3:
                                        Console.WriteLine("Status: Rejected");
                                        break;
                                }
                                if(application.FurtherComments == "")
                                {
                                    Console.WriteLine($"Further Comments: None");
                                } else
                                {
                                    Console.WriteLine($"Further Comments: {application.FurtherComments}");

                                }


                                Console.WriteLine("---------------------------");
                                Console.WriteLine("");
                            }
                        }
                        Console.WriteLine("Press a key to continue.");
                        Console.ReadKey();
                        }
                        catch (FormatException ex) // only exceptions related when there's normally a conversion
                        {
                            Console.WriteLine("Invalid format inputted. Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("There was an error while communicating with the database. It might be that you have input incorrect values. Check again");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Some inputs may be invalid, Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        break;

                    case 5:
                        try { 
                        Console.Clear();
                        Console.WriteLine("Input Username: ");
                        string un = Console.ReadLine();
                        Console.WriteLine("Input Password: ");
                        string pa = Console.ReadLine();

                        var auth = er.GetAuthorizer(un, pa);

                        bool match = false;

                        foreach (var aut in auth)
                        {

                            Console.WriteLine("Press a key to continue.");
                            Console.ReadKey();
                            if (pa == aut.Password)
                            {
                                Console.WriteLine("Password Correct");
                                match = true;
                            }
                            else
                            {
                                Console.WriteLine("Password Incorrect");
                            }
                        }
                        if (match)
                        {
                            int appIdChoice = 0;
                            do
                            {
                                Console.Clear();
                                var pending = lar.GetPendingLeaveApplications();
                                foreach (var temp in pending)
                                {
                                    Console.WriteLine($"Id: {temp.ID}");
                                    Console.WriteLine($"Leave Type: {temp.LeaveType.Title}");
                                    Console.WriteLine($"Hours Booked: {temp.HoursBooked}");
                                    Console.WriteLine($"Date From: {temp.DateFrom}");
                                    Console.WriteLine($"Date To: {temp.DateTo}");
                                }

                                Console.WriteLine("Enter leave application id to see further details: ");
                                Console.WriteLine("Enter 0 to exit.");
                                appIdChoice = Convert.ToInt32(Console.ReadLine());
                                int appId = 0;
                                int lAppChoice = 0;

                                foreach (var temp1 in pending)
                                {
                                    if (temp1.ID == appIdChoice)
                                    {
                                        Console.WriteLine($"Id: {temp1.ID}");
                                        Console.WriteLine($"Employee Id: {temp1.EmployeeIdFK}");
                                        Console.WriteLine($"Employee Name: {temp1.Employee.Name}");
                                        Console.WriteLine($"Employee Surname: {temp1.Employee.Surname}");
                                        Console.WriteLine($"Leave Type: {temp1.LeaveType.Title}");
                                        Console.WriteLine($"Hours Booked: {temp1.HoursBooked}");
                                        Console.WriteLine($"Date From: {temp1.DateFrom}");
                                        Console.WriteLine($"Date To: {temp1.DateTo}");
                                        //2 Accepted 3 Rejected 
                                        if (temp1.FurtherComments == "")
                                        {
                                            Console.WriteLine($"Further Comments: None");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Further Comments: {temp1.FurtherComments}");
                                        }

                                        Console.WriteLine("Enter 2 if application is accepted \nEnter 3 if application is rejeceted");
                                        lAppChoice = Convert.ToInt32(Console.ReadLine());
                                        //lar.ChangeApplicationStatus(temp1.ID, lAppChoice);
                                        appId = temp1.ID;
                                        decimal remainingEntitledHours = Decimal.Subtract(temp1.LeaveType.HoursEntitled, temp1.HoursBooked);

                                        Console.WriteLine($"{temp1.LeaveType.Title} with {remainingEntitledHours} left for {temp1.Employee.Name}{temp1.Employee.Surname}");
                                        //application id, email, status
                                        string emailText = $"Application: {temp1.ID} has been ";
                                        if (lAppChoice == 2)
                                        {
                                            emailText += "Authorized";
                                        } else
                                        {
                                            emailText += "Rejected";
                                        }
                                        
                                        er.SendEmail(temp1.Employee.Email, emailText);
                                        Console.WriteLine("Email sent");

                                        Console.WriteLine("Press a key to continue.");
                                        Console.ReadKey();
                                        break;
                                    }
                                }
                                if(appId != 0)
                                {
                                    lar.ChangeApplicationStatus(appId, lAppChoice);
                                }
                                

                            } while (appIdChoice != 0);
                        }
                                                        
                        Console.WriteLine("Press a key to continue.");
                        Console.ReadKey();
                        }
                        catch (FormatException ex) // only exceptions related when there's normally a conversion
                        {
                            Console.WriteLine("Invalid format inputted. Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("There was an error while communicating with the database. It might be that you have input incorrect values. Check again");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Some inputs may be invalid, Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        break;

                    case 6:
                        try
                        {
                            Random rnd = new Random();
                        int num = rnd.Next(100000, 999999);
                        
                        Console.WriteLine("Enter Id of user to reset password: ");
                        int z = Convert.ToInt32(Console.ReadLine());

                        Employee emp = er.GetEmployee(z);                       

                        
                            er.SendEmail(num, emp.Email);
                            Console.WriteLine("Email Sent");

                            Console.WriteLine("Enter Code sent: ");
                            int verify = Convert.ToInt32(Console.ReadLine());
                            if(verify == num)
                            {
                                Console.WriteLine("Enter new Password");
                                string newPas =Console.ReadLine();
                                Console.WriteLine("Confirm new Password");
                                string confPas = Console.ReadLine();

                                if(confPas == newPas) 
                                {
                                    Console.WriteLine("Password matched");
                                    er.ChangePassword(z, newPas);
                                }
                            }
                        }
                        catch (FormatException ex) // only exceptions related when there's normally a conversion
                        {
                            Console.WriteLine("Invalid format inputted. Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("There was an error while communicating with the database. It might be that you have input incorrect values. Check again");
                            Logger.LogError(ex, "An error occurred");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Some inputs may be invalid, Check your inputs");
                            Logger.LogError(ex, "An error occurred");
                        }

                        Console.WriteLine("Press a key to continue.");
                        Console.ReadKey();

                        break;                     

                }

            } while (mainMenuChoice != 7);
        }
    }
}
