using UnityEngine;

public class Chest : MonoBehaviour, Iinteractable  

{
    public  bool IsOpened { get; private set; }
    public  string ChestID {get; private set; }
    public  GameObject itemPrefab; 
    public Sprite openSprite;



    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject); 

    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public  void Interact()
    {
        if (!CanInteract()) return; 

    }


    private void OpenChest()
    {
        SetOpen(true);
        if (itemPrefab)
        {
            GameObject droppedItem = Instantilate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BounceEffect>().startBounce();
        }
    }

    public void SetOpened(bool opened)
    {
        if (IsOpened = opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite; 
        }
    }
    
}
