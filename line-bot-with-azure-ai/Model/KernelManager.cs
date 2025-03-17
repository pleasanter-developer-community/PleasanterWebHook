using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace PleasanterWebHook.Model;

public static class KernelManager 
{
    public static Kernel CreateKernel(WebhookSettings settings)
    {
#pragma warning disable SKEXP0001, SKEXP0010 //Experimental(実験段階)であることの警告を非表示

        // Azure OpenAIクライアントのインスタンスを生成
        var openAiClient = new AzureOpenAIClient(
            new Uri(settings.AzureOpenAIEndpoint),
            new AzureKeyCredential(settings.AzureOpenAIKey));

        //ChatCompletionのデプロイ(gpt-4o-mini)を紐づけし、Semantic Kernelのインスタンスを生成
        var kernelBuilder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(settings.ChatDeployment, openAiClient);
        var kernel = kernelBuilder.Build();

        // Azure AI Searchのベクトルストアのインスタンスを生成
        var vectorStore = new AzureAISearchVectorStore(
            new SearchIndexClient(
                new Uri(settings.AzureSearchEndpoint),
                new AzureKeyCredential(settings.AzureSearchKey)));

        // ベクトルストアからコレクションを取得
        var collection = vectorStore.GetCollection<string, Ramen>(settings.VectorStoreIndexName);

        //テキスト埋め込み生成サービスのインスタンスを生成
        var embeddingGenarationService
            = new AzureOpenAITextEmbeddingGenerationService(settings.EmbeddingDeployment, openAiClient);

        //VectorStoreTextSearch オブジェクトの生成
        var textSearch = new VectorStoreTextSearch<Ramen>(
            collection,
            embeddingGenarationService,
            null,
            new RamenTextSearchResultMapper(settings.ServiceUrl));

        //VectorStoreTextSearchオブジェクトからファンクション`GetTextSearchResult` を生成
        //そのファンクションを実行するカーネルプラグインを作成
        var searchPlugin = KernelPluginFactory.CreateFromFunctions(
            "SearchPlugin", "ramen search",
            [textSearch.CreateGetTextSearchResults(searchOptions: new TextSearchOptions() { Top = 10 })]);

        //プラグインをカーネルに追加
        kernel.Plugins.Add(searchPlugin);
        return kernel;

#pragma warning restore SKEXP0001, SKEXP0010
    }

    public static async Task<string?> InvokePromptAsync(Kernel kernel, string text)
    {
        //プロンプトテンプレート構築用のファクトリクラス
        //- ここではHandlebarsテンプレートエンジンを利用
        var promptTemplateFactory = new HandlebarsPromptTemplateFactory();
        //検索結果からプロンプトを生成するテンプレートの定義
        //- <Plugin名>-<Function名> でカーネルプラグインのファンクションを呼び出し
        //- 結果を {{#each this}} で反復処理
        var promptTemplate = """
                {{#with (SearchPlugin-GetTextSearchResults query)}}  
                    {{#each this}}  
                    Name: {{Name}}
                    Value: {{Value}}
                    Link: {{Link}}
                    -----------------
                    {{/each}}  
                {{/with}}  

                {{query}}

                Include citations to the relevant information where it is referenced in the response.
                """;
        var result = await kernel.InvokePromptAsync(
            promptTemplate,
            new KernelArguments() { { "query", text } },
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory);
        return result?.GetValue<string>();
    }
}