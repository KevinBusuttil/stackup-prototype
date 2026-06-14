using System.Collections;
using UnityEngine;

public class StackGameManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Vector2 spawnPosition = new Vector2(0f, 3.5f);
    public float spawnDelay = 0.5f;

    private BlockController currentBlock;
    private bool isSpawning;

    private void Start()
    {
        SpawnBlock();
    }

    private void Update()
    {
        if (currentBlock != null && currentBlock.IsSettled && !isSpawning)
        {
            StartCoroutine(SpawnNextBlock());
        }
    }

    private IEnumerator SpawnNextBlock()
    {
        isSpawning = true;
        currentBlock = null;

        yield return new WaitForSeconds(spawnDelay);

        SpawnBlock();
        isSpawning = false;
    }

    private void SpawnBlock()
    {
        GameObject block = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);
        currentBlock = block.GetComponent<BlockController>();
    }
}