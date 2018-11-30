using UnityEngine;

public class slFoodPresentation : MonoBehaviour 
{
	public slConstants.FoodType FoodType;
	public SpriteRenderer BackSprite;
	public SpriteRenderer FrontSprite;

	public void SetColor(Color color)
	{
		switch (FoodType)
		{
			case slConstants.FoodType.Normal:
				BackSprite.color = color;
				break;
			case slConstants.FoodType.Remains:
				BackSprite.color = color;
				FrontSprite.color = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
				break;
		}
	}
}