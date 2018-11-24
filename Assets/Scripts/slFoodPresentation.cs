using UnityEngine;

public class slFoodPresentation : MonoBehaviour 
{
	public SpriteRenderer NormalSprite;
	public SpriteRenderer LargeSprite;
	public SpriteRenderer LargeInSprite;

	public void ChangeFoodType(slFood.FoodProperties foodProperties, Color color)
	{
		switch (foodProperties.MyType)
		{
			case slFood.FoodType.Normal:
				NormalSprite.enabled = true;
				LargeSprite.enabled = false;
				LargeInSprite.enabled = false;

				NormalSprite.color = color;
				break;
			case slFood.FoodType.Large:
				NormalSprite.enabled = false;
				LargeSprite.enabled = true;
				LargeInSprite.enabled = true;

				LargeSprite.color = color;
				LargeInSprite.color = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
				break;
		}
	}
}