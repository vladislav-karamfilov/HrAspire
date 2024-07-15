namespace HrAspire.Data.Common;

public interface IDeletableEntity
{
    bool IsDeleted { get; set; }

    DateTime? DeletedOn { get; set; }
}
