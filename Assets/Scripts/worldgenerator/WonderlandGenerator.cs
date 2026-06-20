using UnityEngine;

public static class WonderlandGenerator
{
    // recebe as coordenadas globais e devolve a altura do terreno
    // define o tipo de bloco que deve estar na superfície para cada bioma.
    public static void GetBiomeData(float wx, float wz, out float finalHeight, out Block.BlockType surfaceBlock)
    {
        // Calculo do Ruído
        // x, z, octaves, scale, persistence, lacunarity
        float magicNoise = NoiseUtils.FBm(wx, wz, 1, 0.005f);
        //float magicNoise = 0.8f; // debug

        if (magicNoise < 0.45f)
        {
            // --- ZONA NORMAL ---
/*             float continentalness = NoiseUtils.FBm(wx, wz, 2, 0.02f);
            float baseHeight = Mathf.Pow(NoiseUtils.FBm(wx, wz, 4, 0.02f), 0.5f);
            float detail = NoiseUtils.FBm(wx, wz, 6, 0.1f);
            
            finalHeight = Mathf.Lerp(4f, 14f, continentalness * baseHeight) + detail * 2f;
            surfaceBlock = Block.BlockType.GRASS; */

            float continentalness = NoiseUtils.FBm(wx, wz, 2, 0.01f); 
            float baseHeight = Mathf.Pow(NoiseUtils.FBm(wx, wz, 4, 0.015f), 0.5f);
            float detail = NoiseUtils.FBm(wx, wz, 6, 0.03f); // Menos detalhe abrupto
            
            finalHeight = Mathf.Lerp(4f, 9f, continentalness * baseHeight) + detail * 1f; 
            surfaceBlock = Block.BlockType.GRASS;
        }
        else if (magicNoise >= 0.45f && magicNoise < 0.55f)
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
            // --- ZONA DOS COGUMELOS ---
            // frequência alta no baseHeight para criar reentrancias no terreno
            float baseBumps = Mathf.Pow(NoiseUtils.FBm(wx, wz, 3, 0.08f), 2f);
            
            // O chão base vai oscilar entre 3 e 8 de altura
            finalHeight = Mathf.Lerp(3f, 8f, baseBumps); 
            
            // O chão do bosque pode ser DIRT
            surfaceBlock = Block.BlockType.DIRT; 

        }
    }
}