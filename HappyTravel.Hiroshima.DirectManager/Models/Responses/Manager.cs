﻿using HappyTravel.Hiroshima.Common.Models.Enums;

namespace HappyTravel.Hiroshima.DirectManager.Models.Responses
{
    public readonly struct Manager
    {
        public Manager(string firstName, string lastName, string title, string position, string email, string phone, string fax, ManagerPermissions permissions, bool isMaster)
        {
            FirstName = firstName;
            LastName = lastName;
            Title = title;
            Position = position;
            Email = email;
            Phone = phone;
            Fax = fax;
            Permissions = permissions;
            IsMaster = isMaster;
        }
        
        
        public string FirstName { get; }
        public string LastName { get; }
        public string Title { get; }
        public string Position { get; }
        public string Email { get; }
        public string Phone { get; }
        public string Fax { get; }
        public ManagerPermissions Permissions { get; }
        public bool IsMaster { get; }
    }
}