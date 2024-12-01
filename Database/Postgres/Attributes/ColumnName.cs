namespace Postgres.Attributes;

public class ColumnName : Attribute
{
    public string Name { get; set; }

    public ColumnName(string name)
    {
        Name = name;
    }
}