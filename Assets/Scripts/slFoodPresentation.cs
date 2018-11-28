using UnityEngine;

public class slFoodPresentation : MonoBehaviour 
{
	public slFood.FoodType FoodType;
	public SpriteRenderer BackSprite;
	public SpriteRenderer FrontSprite;

	public void SetColor(Color color)
	{
		switch (FoodType)
		{
			case slFood.FoodType.Normal:
				BackSprite.color = color;
				break;
			case slFood.FoodType.Remains:
				BackSprite.color = color;
				FrontSprite.color = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
				break;
		}
	}
}