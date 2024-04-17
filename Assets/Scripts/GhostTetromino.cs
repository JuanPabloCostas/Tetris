using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTetromino : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        tag = "currentGhostTetromino";
        foreach (Transform mino in transform)
        {
            mino.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        FollowActiveTetromino();
        MoveDown();
    }

    void FollowActiveTetromino()
    {
        Transform currentActiveTetrominoTransform = GameObject.FindGameObjectWithTag("currentActiveTetromino").transform;

        transform.position = currentActiveTetrominoTransform.position;
        transform.rotation = currentActiveTetrominoTransform.rotation;
    }

    void MoveDown()
    {
        while(CheckIsValidPosition())
        {
            transform.position += new Vector3(0, -1, 0);
        }

        if(!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
        }
    }

    bool CheckIsValidPosition()
    {
        foreach (Transform mino in transform)
        {
            Vector2 pos = FindAnyObjectByType<Game>().Round(mino.position);
            if (FindAnyObjectByType<Game>().CheckIsInsideGrid(pos) == false)
            {
                return false;
            }
            if (FindAnyObjectByType<Game>().GetTransformAtGridPosition(pos) != null && FindAnyObjectByType<Game>().GetTransformAtGridPosition(pos).parent.tag == "currentActiveTetromino")
            {
                return true;
            }
            if (FindAnyObjectByType<Game>().GetTransformAtGridPosition(pos) != null && FindAnyObjectByType<Game>().GetTransformAtGridPosition(pos).parent != transform)
            {
                return false;
            }
        }
        return true;

    }
}
