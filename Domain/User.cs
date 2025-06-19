namespace WTW.AuthenticationService.Domain
{
    public class User
    {
        protected User() { }

        public User(string userName, string businessGroup, string referenceNumber, string status, string type)
        {
            UserName = userName;
            BusinessGroup = businessGroup;
            ReferenceNumber = referenceNumber;
            Status = status;
            Type = type;
        }

        public string UserName { get; }
        public string BusinessGroup { get; }
        public string ReferenceNumber { get; }
        public string Status { get; }
        public string Type { get; }

        public bool IsValidBusinessGroup(IEnumerable<string> businessGroups)
        {
            return businessGroups.Contains(BusinessGroup);
        }

        public bool IsAdmin()
        {
            return Type == "A";
        }
    }
}