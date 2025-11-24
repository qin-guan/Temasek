using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace Temasek.Calendarr.Services;

public class SemanticComparator : IDisposable
{
    private const int HiddenSize = 384;

    private readonly InferenceSession _session;
    private readonly BertTokenizer _tokenizer;

    public SemanticComparator(string modelPath, string vocabPath)
    {
        var options = new Microsoft.ML.OnnxRuntime.SessionOptions
        {
            IntraOpNumThreads = 1,
            InterOpNumThreads = 1,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            EnableCpuMemArena = true,
        };

        try
        {
            _session = new InferenceSession(modelPath, options);
            var bertOptions = new BertOptions { LowerCaseBeforeTokenization = true };
            _tokenizer = BertTokenizer.Create(vocabPath, bertOptions);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load model resources", ex);
        }
    }

    public float Compare(string text1, string text2)
    {
        var vec1 = GetEmbedding(text1);
        var vec2 = GetEmbedding(text2);
        return CosineSimilarity(vec1, vec2);
    }

    private float[] GetEmbedding(string text)
    {
        var tokenIds = _tokenizer.EncodeToIds(text);
        var count = tokenIds.Count + 2;
        var inputIds = new long[count];
        var attentionMask = new long[count];
        var tokenTypeIds = new long[count];

        inputIds[0] = 101;
        attentionMask[0] = 1;
        tokenTypeIds[0] = 0;

        for (var i = 0; i < tokenIds.Count; i++)
        {
            inputIds[i + 1] = tokenIds[i];
            attentionMask[i + 1] = 1;
            tokenTypeIds[i + 1] = 0;
        }

        inputIds[count - 1] = 102;
        attentionMask[count - 1] = 1;
        tokenTypeIds[count - 1] = 0;

        var dimensions = new[] { 1, count };
        var inputIdsTensor = new DenseTensor<long>(inputIds, dimensions);
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, dimensions);
        var tokenTypeIdsTensor = new DenseTensor<long>(tokenTypeIds, dimensions);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor),
        };

        using var results = _session.Run(inputs);
        var outputTensor = results[0].AsTensor<float>();

        return MeanPooling(outputTensor, count);
    }

    private static float[] MeanPooling(Tensor<float> lastHiddenState, int seqLength)
    {
        var pooled = new float[HiddenSize];

        for (var i = 0; i < seqLength; i++)
        {
            for (var j = 0; j < HiddenSize; j++)
            {
                pooled[j] += lastHiddenState[0, i, j];
            }
        }

        for (var j = 0; j < HiddenSize; j++)
        {
            pooled[j] /= seqLength;
        }

        float sumSq = 0;
        for (var j = 0; j < HiddenSize; j++)
        {
            sumSq += pooled[j] * pooled[j];
        }

        var norm = (float)Math.Sqrt(sumSq);
        if (!(norm > 1e-12))
            return pooled;
        {
            for (var j = 0; j < HiddenSize; j++)
            {
                pooled[j] /= norm;
            }
        }

        return pooled;
    }

    private static float CosineSimilarity(float[] v1, float[] v2)
    {
        return v1.Select((t, i) => t * v2[i]).Sum();
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}
