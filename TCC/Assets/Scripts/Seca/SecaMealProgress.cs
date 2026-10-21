using UnityEngine;

public class SecaMealProgress : MonoBehaviour
{
    [Header("Conversa da refeição")]
    [SerializeField] private SecaMealDialogue mealDialogue;

    public void RegisterMeal()
    {
        if (mealDialogue == null)
        {
            Debug.LogError(
                "SecaMealProgress: arraste a Mesa para Meal Dialogue.",
                this
            );
            return;
        }

        if (!mealDialogue.MealCompleted)
        {
            Debug.LogWarning(
                "A refeição ainda não terminou.",
                this
            );
            return;
        }

        int choice = mealDialogue.FoodChoiceIndex;

        if (choice < 0 || choice > 2)
        {
            Debug.LogError(
                "SecaMealProgress: a refeição terminou sem uma escolha válida.",
                this
            );
            return;
        }

        GameProgress progress = GameProgress.Instance;

        if (progress == null)
        {
            Debug.LogError(
                "SecaMealProgress: não existe GameProgress ativo na partida.",
                this
            );
            return;
        }

        progress.NormalizeData();

        // Converte os índices dos botões para os valores do progresso.
        progress.Data.secaFoodChoice = choice + 1;
        progress.Data.secaMealCompleted = true;

        string description;

        switch (choice)
        {
            case 0:
                description = "Comeu tudo";
                break;

            case 1:
                description = "Dividiu com Baleia";
                break;

            default:
                description = "Dividiu com a irmã";
                break;
        }

        Debug.Log(
            "Refeição registrada no progresso: " + description,
            this
        );
    }
}