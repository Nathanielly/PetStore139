using System.Formats.Asn1;

namespace Models;
public class PetModel
{
    public int id;
    public Category category;
    public string name;
    public string[] photoUrls;
    public Tag[] tags;
    public string status;

}

public class Category
{
    public int id;
    public string name;

    //construtor
    public Category (int id, string name)
    {
        this.id = id;
        this.name = name;
    }

}

public class Tag
{
    public int id;
    public string name;

    public Tag (int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}