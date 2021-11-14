using UnityEngine;

public class BarPuzzle : MonoBehaviour
{
    
    // Define Puzzle Tiles
    public GameObject[] paintingTiles;

    // Define correct order for the puzzle
    private string[] order = { "BarPuzzleTile3",
                              "BarPuzzleTile6",
                              "BarPuzzleTile7",
                              "BarPuzzleTile2",
                              "BarPuzzleTile1",
                              "BarPuzzleTile5",
                              "BarPuzzleTile4"};
    // each tile's state to determine if it's in its correct place
    public bool[] tilesStates = new bool[7];
    // materials for each tile when they are in their right place
    public Material[] solvedMats;

    public Material solvedMat;
    // to keep track if two tiles have been swapped one time
    private bool swapped = false;
    // to determine if the puzzle is solved
    public bool solved = false;

    void Start()
    {
        // Rondomize the tiles
        paintingTiles = RandomizeArray(paintingTiles);

        // place the tiles on the puzzle table 
        for (int i = 0; i < paintingTiles.Length; i++)
        {
            // if it's the portrait ones
            if (paintingTiles[i].CompareTag("BarPuzzleTile3") || paintingTiles[i].CompareTag("BarPuzzleTile4"))
            {   
                // instantiate them with specific euler
                Instantiate(paintingTiles[i], transform.GetChild(i).position,
                    Quaternion.Euler(0, -90, -15.384f), transform.GetChild(i));
            }
            else
            {
                Instantiate(paintingTiles[i], transform.GetChild(i).position, transform.GetChild(i).rotation, transform.GetChild(i));
            }
        }
        // Check tiles to see any of them has gone in their right place
        CheckTiles();
    }

    // Swap tiles with each other (is called from first person controller class)
    public void SwapTilesInArray(GameObject firstTile, GameObject secondTile)
    {
        GameObject tmpTile; // to save one of the elements in there for swapping
        swapped = false; // has not yet been swapped

        for (int i = 0; i < paintingTiles.Length; i++)
        {
            for (int j = 0; j < paintingTiles.Length; j++)
            {   
                // if the first loop has reached the first tile and the other to the second one
                // and they haven't been swapped yet
                if (paintingTiles[i].tag == firstTile.tag
                    && paintingTiles[j].tag == secondTile.tag
                    && swapped == false)
                {
                    // Save second tile in the temp
                    tmpTile = paintingTiles[j];
                    // assign first tile to the second one
                    paintingTiles[j] = paintingTiles[i];
                    // assign temp to the first tile
                    paintingTiles[i] = tmpTile;
                    // Set swapped to true to break the loops
                    swapped = true;
                }
            }
        }
        // Check the tiles oreder after they have been swapped
        CheckTiles();
    }

    // Check if any of the tiles is in its right place
    private void CheckTiles()
    {
        for (int i = 0; i < paintingTiles.Length; i++)
        {
            // Check the tile if it hasn't been previously checked as a correct one
            if (tilesStates[i] == false)
            {
                // Check if curent tile in at the index is matching the tag in the order array
                if (paintingTiles[i].tag == order[i])
                {
                    // Change number material to solved material
                    transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = solvedMat;
                    // Set the state of the tile to true
                    tilesStates[i] = true;
                }
            }
        }
        // Check if all of the tiles are in their correct place and if so mark puzzle as solved

        int solvedTiles = 0; // keep track of correct tiles
        for (int i = 0; i < tilesStates.Length; i++)
        {
            // if tile is in its correct place
            if (tilesStates[i])
            {
                // increment the number of solved tiles
                solvedTiles++;
            }
        }
        // if solved tiles number is equal to the number of all tiles
        if (solvedTiles == 7)
        {
            // mark puzzle as solved
            solved = true;
        }
    }

    // Check if the tile that player is looking at is in its correct place and is locked (called by first person controller class)
    public bool IsLocked(string tag)
    {
        // find the specfic tile in the array
        for (int i = 0; i < order.Length; i++)
        {
            if (tag == order[i])
            {
                // if the tile is in the right place
                if (tilesStates[i])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    // Randomize and Shuffle tiles for a new order of placement
    private GameObject[] RandomizeArray(GameObject[] array)
    {
        int arrIndex = 0;
        // start from the last index
        for (int i = array.Length - 1; i > 0; i--)
        {
            do
            {
                // set a random number between first element and the current index
                arrIndex = Random.Range(0, i);
                if (i < 2 && array[arrIndex].tag == order[i])
                {
                    array = RandomizeArray(array);
                    return array;
                    //break;
                }
            }
            while (array[arrIndex].tag == order[i]);
            // create temp for holding element for swap
            GameObject tmpTile = array[i];
            // swap element at the index with the random index
            array[i] = array[arrIndex];
            // assign temp to element with random index
            array[arrIndex] = tmpTile;
        }

        // Number of correct tiles
        int correctTiles = 0;
        // Loop through the array to see how many elements are in their correct place
        for (int i = 0; i < array.Length; i++)
        {
            // If any element is in its correct place
            if (array[i].name == order[i])
            {
                // Increment correct tiles
                correctTiles++;
            }
        }

        // If there is no correct tile at the beginning of the game 
        if (correctTiles == 0)
        {
            // Choose one of the tiles randomly
            arrIndex = Random.Range(0, 7);
            
            int targetElem = 0;
            // Find the right index for that randomly selected element
            for (int i = 0; i < order.Length; i++)
            {
                // Save its index
                if (order[i] == array[arrIndex].name)
                {
                    targetElem = i;
                }
            }

            // Swap the tiles
            GameObject tmpCTile = array[targetElem];
            array[targetElem] = array[arrIndex];
            array[arrIndex] = tmpCTile;
        }
        
        return array;
    }
}
