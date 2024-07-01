
using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Models;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class BookControoler
{
     // Cria uma resposta padrão para as requisições do API Gateway.
    private APIGatewayProxyResponse GetDefaultResponse()
    {
        var response = new APIGatewayProxyResponse()
        {
            Headers = new Dictionary<string, string>(),
            StatusCode = 200
        };

        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Headers", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST, GET, PUT, DELETE");
        response.Headers.Add("Content-Type", "application/json");

        return response;
    }
 // Obtém o nome da região do AWS.
    private string GetRegionName() =>
        Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";

 // Salva um novo livro na tabela do DynamoDB.
    public async Task<APIGatewayProxyResponse> SaveBook(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Deserializa o objeto 'Book' a partir do corpo da requisição.
        var book = JsonSerializer.Deserialize<Book>(request.Body);

        // Conecta ao DynamoDB
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));

        using (var dbContext = new DynamoDBContext(dbClient))
            await dbContext.SaveAsync(book);

        var response = GetDefaultResponse();

        response.Body = JsonSerializer.Serialize(new { Message = "Book saved successfully!" });

        return response;
    }

    public async Task<APIGatewayProxyResponse> GetBook (APIGatewayProxyRequest request, ILambdaContext context)
    {
        var bookId = request.PathParameters["bookId"];
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
        using (var dbContext = new DynamoDBContext(dbClient)){
            var book = await dbContext.LoadAsync<Book>(bookId);
            if (book != null){
                var response = GetDefaultResponse();
                response.Body = JsonSerializer.Serialize(book);
                return response;
            }
            return new APIGatewayProxyResponse{
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = JsonSerializer.Serialize(new {Message = "Book Not Found"})
            };
            }
      
    }
    public async Task<APIGatewayProxyResponse> GetAllBooks(APIGatewayProxyRequest request, ILambdaContext context)
{
    var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
    using (var dbContext = new DynamoDBContext(dbClient))
    {
       
        var books = await dbContext.ScanAsync<Book>(new List<ScanCondition>()).GetRemainingAsync();

        if (books != null)
        {
            var response = GetDefaultResponse();
            response.Body = JsonSerializer.Serialize(books);
            return response;
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.NotFound,
            Body = JsonSerializer.Serialize(new { Message = "No books found" })
        };
    }
}


    public async Task<APIGatewayProxyResponse> UpdateBook (APIGatewayProxyRequest request, ILambdaContext context){
        var bookId = request.PathParameters["bookId"];
        var updateBook = JsonSerializer.Deserialize<Book>(request.Body);
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
        
        using (var dbContext = new DynamoDBContext(dbClient)){

            var book = await dbContext.LoadAsync<Book>(bookId);
            if (book != null){
                book.Author = updateBook.Author;
                book.Name=updateBook.Name;
                book.Price = updateBook.Price;
                book.Rating = updateBook.Rating;
                await dbContext.SaveAsync(book);

                var response = GetDefaultResponse();
                response.Body = JsonSerializer.Serialize(new { Message = "Book updated!"});
                return response;
            }
            return new APIGatewayProxyResponse{
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = JsonSerializer.Serialize(new {Message = "Book Not Found"})
            };
            }

    }
    public async Task<APIGatewayProxyResponse> DeleteBook (APIGatewayProxyRequest request, ILambdaContext context){
        var bookId = request.PathParameters["bookId"];
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
        using (var dbContext = new DynamoDBContext(dbClient)) {
            var book = await dbContext.LoadAsync<Book>(bookId);

            if(book != null){
                await dbContext.DeleteAsync(book);

                var response = GetDefaultResponse();
                response.Body = JsonSerializer.Serialize(new { Message = "Book deleted!"});
                return response;
            }
        }
        return new APIGatewayProxyResponse{
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = JsonSerializer.Serialize(new {Message = "Book Not Found"})};

    }
    }

   
