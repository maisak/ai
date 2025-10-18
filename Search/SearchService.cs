using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Search.Models;

namespace Search;

public class SearchService(SearchIndexClient indexClient, SearchClient searchClient)
{
	public async Task IndexAsync(string documentName, CancellationToken cancellationToken)
	{
		var document = new FileRecord(documentName);
		await searchClient.UploadDocumentsAsync([document], cancellationToken: cancellationToken);
	}

	public async Task<string> GetStats()
	{
		Response<SearchServiceStatistics> stats = await indexClient.GetServiceStatisticsAsync();
		
		return $"You are using {stats.Value.Counters.IndexCounter.Usage} indexes.";
	}

	public async Task Search()
	{
		var options = new SearchOptions
		{
			IncludeTotalCount = true,
			Filter = "",
			OrderBy = { "" }
		};
		
		var response = await searchClient.SearchAsync<SearchDocument>("*", options);
	}

	public async Task DeleteAllDocuments()
	{
		var response = await searchClient.SearchAsync<SearchDocument>("*", new SearchOptions { IncludeTotalCount = true });         
		var docs = response.Value.GetResults().Select(x => x.Document);          
		await searchClient.DeleteDocumentsAsync(docs);
	}
	
	public async Task CreateIndex()
	{
		const string indexName = "hotels";
		var index = new SearchIndex(indexName)
		{
			Fields =
			{
				new SimpleField("HotelId", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true },
				new SearchableField("HotelName") { IsFilterable = true, IsSortable = true },
				new SearchableField("Description") { AnalyzerName = LexicalAnalyzerName.EnLucene },
				new SearchableField("Tags", collection: true) { IsFilterable = true, IsFacetable = true },
				new ComplexField("Address")
				{
					Fields =
					{
						new SearchableField("StreetAddress"),
						new SearchableField("City") { IsFilterable = true, IsSortable = true, IsFacetable = true },
						new SearchableField("StateProvince") { IsFilterable = true, IsSortable = true, IsFacetable = true },
						new SearchableField("PostalCode") { IsFilterable = true, IsSortable = true, IsFacetable = true }
					}
				}
			}
		};

		await indexClient.CreateIndexAsync(index);
	}
}