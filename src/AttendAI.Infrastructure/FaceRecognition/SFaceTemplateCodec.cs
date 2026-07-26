using System.Buffers.Binary;

namespace AttendAI.Infrastructure.FaceRecognition;

public static class SFaceTemplateCodec
{
    public static byte[] Serialize(ReadOnlySpan<float> embedding)
    {
        ValidateEmbedding(embedding);

        var normalized = Normalize(embedding);
        var bytes = new byte[normalized.Length * sizeof(float)];
        for (var index = 0; index < normalized.Length; index++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(
                bytes.AsSpan(index * sizeof(float), sizeof(float)),
                normalized[index]);
        }

        return bytes;
    }

    public static float[] Deserialize(ReadOnlySpan<byte> bytes, int expectedDimension)
    {
        if (expectedDimension <= 0 ||
            bytes.Length != expectedDimension * sizeof(float) ||
            bytes.Length % sizeof(float) != 0)
        {
            throw new InvalidOperationException("Stored SFace template has an invalid byte length.");
        }

        var embedding = new float[expectedDimension];
        for (var index = 0; index < embedding.Length; index++)
        {
            embedding[index] = BinaryPrimitives.ReadSingleLittleEndian(
                bytes.Slice(index * sizeof(float), sizeof(float)));
        }

        ValidateEmbedding(embedding);
        return Normalize(embedding);
    }

    public static float[] Normalize(ReadOnlySpan<float> embedding)
    {
        ValidateEmbedding(embedding);

        double normSquared = 0;
        for (var index = 0; index < embedding.Length; index++)
        {
            normSquared += embedding[index] * embedding[index];
        }

        var norm = Math.Sqrt(normSquared);
        if (!double.IsFinite(norm) || norm <= 0)
        {
            throw new InvalidOperationException("SFace embedding has an invalid norm.");
        }

        var normalized = new float[embedding.Length];
        for (var index = 0; index < embedding.Length; index++)
        {
            normalized[index] = (float)(embedding[index] / norm);
        }

        return normalized;
    }

    private static void ValidateEmbedding(ReadOnlySpan<float> embedding)
    {
        if (embedding.Length == 0)
        {
            throw new InvalidOperationException("SFace embedding is empty.");
        }

        for (var index = 0; index < embedding.Length; index++)
        {
            if (!float.IsFinite(embedding[index]))
            {
                throw new InvalidOperationException("SFace embedding contains a non-finite value.");
            }
        }
    }
}
