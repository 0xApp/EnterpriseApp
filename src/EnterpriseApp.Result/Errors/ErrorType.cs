namespace EnterpriseApp.Errors;

public enum ErrorType : ushort
{
    NotFound = 1,
    BusinessRuleViolation = 2,
    ValidationError = 3,
    Unauthorized = 4,
    Forbidden = 5,
    
    SystemError = 6,
    ServiceUnavailable = 7,
    ServiceError = 8,
}