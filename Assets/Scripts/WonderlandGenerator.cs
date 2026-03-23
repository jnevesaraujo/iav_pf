using UnityEngine;

public static class WonderlandGenerator
{
    // recebe as coordenadas globais e devolve a altura do terreno
    // define o tipo de bloco que deve estar na superfície para cada bioma.
    public static void GetBiomeData(float wx, float wz, out float finalHeight, out Block.BlockType surfaceBlock)
    {
        // Calculo do Ruído
        float magicNoise = NoiseUtils.FBm(wx, wz, 1, 0.005f);

        if (magicNoise < 0.4f)
        {
            // --- ZONA NORMAL ---
            float continentalness = NoiseUtils.FBm(wx, wz, 2, 0.02f);
            float baseHeight = Mathf.Pow(NoiseUtils.FBm(wx, wz, 4, 0.02f), 0.5f);
            float detail = NoiseUtils.FBm(wx, wz, 6, 0.1f);
            
            finalHeight = Mathf.Lerp(4f, 14f, continentalness * baseHeight) + detail * 2f;
            surfaceBlock = Block.BlockType.GRASS;
        }
        else if (magicNoise >= 0.4f && magicNoise < 0.7f)
        {
            // --- ZONA DO XADREZ ---
            // Altura plana: fixada a altura num valor baixo com apenas um detalhe
            float detail = NoiseUtils.FBm(wx, wz, 6, 0.1f);
            finalHeight = 5f + (detail * 0.5f); 

            // Definição com par/ímpar
            if ((Mathf.FloorToInt(wx) + Mathf.FloorToInt(wz)) % 2 == 0)
                surfaceBlock = Block.BlockType.CHESS_WHITE;
            else
                surfaceBlock = Block.BlockType.CHESS_RED;
        }
        else
        {
            // --- ZONA DOS COGUMELOS TBD ---
            finalHeight = 12f; // placeholder temporário
            surfaceBlock = Block.BlockType.DIRT; // placeholder temporário
        }
    }
}