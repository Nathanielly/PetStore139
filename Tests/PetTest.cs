// 1 - Bibliotecas
using RestSharp;
using Newtonsoft.Json;
using Models;

// 2 - Namespace
namespace Pet;


// 3 - Classes
public class PetTest
{

    // 3.1 - Atributos
    //Endereço da API
    private const string BASE_URL = "https://petstore.swagger.io/v2/";

    // 3.2 - Funções e métodos

    //Função de leitura de dados a partir de um arquivo csv
    public static IEnumerable<TestCaseData> getTestData()
    {
        string caminhoMassa = @"C:\Iterasys\PetStore139\Fixtures\pets.csv";

        using var reader = new StreamReader(caminhoMassa);

        // Pula a primeira linha com os cabeçalhos
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(",");

            yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], values[5], values[6], values[7]);
        }

    }



    [Test, Order(1)]
    public void PostPetTest()
    {
        //Configura (Variaveis e configurações)
        //Instancia o objeto do tipo RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        //instancia o objeto do tipo RestRequest com o complemento de endereço, como "pet" e configurando o método para ser um post.
        var request = new RestRequest("pet", Method.Post);

        //armazena o conteúdo do arquivo pet1.json na memória
        string jsonBody = File.ReadAllText(@"C:\Iterasys\PetStore139\Fixtures\Pet1.json");

        //adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        //Executa
        //executa a requisição conforme a configuração realizada e guarda o json retornado no objeto responde
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        //exibe o response body no console
        Console.WriteLine(responseBody);

        // valide que na resposta, o status code
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        //valida o petId
        int petId = responseBody.id;
        Assert.That(petId, Is.EqualTo(1153178));

        //valida o nome do pet na resposta
        string name = responseBody.name;
        Assert.That(name, Is.EqualTo("Anddy"));

        //valida o status do pet na resposta
        Assert.That(responseBody.status.ToString(), Is.EqualTo("available"));

        //armazenar os dados obtidos para usar nos proximos testes
        Environment.SetEnvironmentVariable("petId", petId.ToString());

    }

    [Test, Order(2)]
    public void GetPetTest()
    {
        //configura
        int petId = 1153178;
        string petName = "Anddy";
        string petCategoryName = "Dog";
        string petTagsName = "vacinado";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Get);

        //executa
        var response = client.Execute(request);

        //valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        Assert.That((string)responseBody.name, Is.EqualTo(petName));
        Assert.That((string)responseBody.category.name, Is.EqualTo(petCategoryName));
        Assert.That((string)responseBody.tags[0].name, Is.EqualTo(petTagsName));
    }

    [Test, Order(3)]
    public void PutPetTest()
    {
        //Configura
        // Os dados de entrada vão formar o body da alteração
        //criar uma classe de modelo
        PetModel petModel = new PetModel();
        petModel.id = 1153178;
        petModel.category = new Category(1, "Dog");
        petModel.name = "Anddy";
        petModel.photoUrls = new string[]{""};
        petModel.tags = new Tag[]{new Tag(1, "vacinado"), new Tag(2, "castrado")};
        petModel.status = "sold";

        //Transformar o modelo acima em arquivo json
        string jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Put);
        request.AddBody(jsonBody);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petModel.id));
        Assert.That((string)responseBody.tags[1].name, Is.EqualTo(petModel.tags[1].name));
        Assert.That((string)responseBody.status, Is.EqualTo(petModel.status));
    }

    [Test, Order(4)]
    public void DeletePetTest()
    {
        //Configura
        string petId = Environment.GetEnvironmentVariable("petId");

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Delete);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        Assert.That((string)responseBody.message, Is.EqualTo(petId));
        
    }
    
    [TestCaseSource("getTestData", new object[]{}), Order(5)]
    public void PostPetDataDriveTest(int petId, int categoryId, string categoryName, string petName, string photoUrls, string tagsIds, string tagsName, string status)
    {
        //Configura (Variaveis e configurações)
        PetModel petModel = new PetModel();
        petModel.id = petId;
        petModel.category = new Category(categoryId, categoryName);
        petModel.name = petName;
        petModel.photoUrls = new String[]{photoUrls};

        //código para gerar as múltiplas tags que o pet pode ter 
        string[] tagsIdsList = tagsIds.Split(";"); //ler
        string[] tagsNameList = tagsName.Split(";"); //ler
        List<Tag> tagList = new List<Tag>(); //gravar depois do for

        for (int i = 0; i < tagsIdsList.Length; i++)
        {
            int tagId = int.Parse(tagsIdsList[i]);
            string tagName = tagsNameList[i];

            Tag tag = new Tag(tagId, tagName);
            tagList.Add(tag);
        }


        petModel.tags = tagList.ToArray();
        petModel.status = status;

        //estrtura de dados está pronta, agora vamos serializar
        string jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        //Instancia o objeto do tipo RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        //instancia o objeto do tipo RestRequest com o complemento de endereço, como "pet" e configurando o método para ser um post.
        var request = new RestRequest("pet", Method.Post);

        //adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        //Executa
        //executa a requisição conforme a configuração realizada e guarda o json retornado no objeto responde
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        //exibe o response body no console
        //Console.WriteLine(responseBody);

        // valide que na resposta, o status code
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        //valida o petId
        //int petId = responseBody.id;
        Assert.That((int)responseBody.id, Is.EqualTo(petId));

        //valida o nome do pet na resposta
        string name = responseBody.name;
        Assert.That((string)responseBody.name, Is.EqualTo(petName));

        //valida o status do pet na resposta
        Assert.That((string)responseBody.status, Is.EqualTo(status));


    }

    public void GetUserLoginTest()
    {
        //configura
        string username = "nath";
        string password = "nath";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"user/login?username={username}&password{password}", Method.Get);

        //executa
        var response = client.Execute(request);

        //valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        string message = responseBody.message;
        string token = message.Substring(message.LastIndexOf(":")+1);
        Console.WriteLine($"Token = {token}");

        Environment.SetEnvironmentVariable("token", token);
        


    }

}
