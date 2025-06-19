namespace WTW.AuthenticationService.Domain;

public class EpaUser
{
    public EpaUser(string referenceNumber, string businessGroup)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
    }
    
    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ReferenceNumber) && !string.IsNullOrEmpty(BusinessGroup);
    }
    
    public bool IsValidForUser(User user)
    {
        return !user.IsAdmin() && !IsLinked(user);
    }

    private bool IsLinked(User user)
    {
        return ReferenceNumber == user.ReferenceNumber &&
               BusinessGroup == user.BusinessGroup;
    }
}