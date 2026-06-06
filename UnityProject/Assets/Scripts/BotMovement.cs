using UnityEngine;

public class BotMovement : MonoBehaviour
{
    [Header("Setari Bot")]
    public float vitezaBot = 15f; 
    public float distantaDepasire = 2f; // Cât de departe în față trebuie să fie jucătorul ca să dispară
    
    private Vector3 pozitieStart;
    private Transform playerTransform;
    private Animator animator;

    void Start()
    {
        pozitieStart = transform.position;
        animator = GetComponent<Animator>();
        
        // Căutăm jucătorul în scenă după numele lui
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        // 1. Mișcarea botului (aceeași direcție ca jucătorul)
        transform.Translate(Vector3.forward * -vitezaBot * Time.deltaTime, Space.Self);

        // 2. Logica de dispariție: dacă playerul a trecut de el (pe axa Z)
        if (playerTransform != null)
        {
            // Verificăm dacă playerul e cu "distantaDepasire" în fața botului
            // Folosim Math.Abs sau comparație directă dacă axa este constantă
            if (playerTransform.position.z < transform.position.z - distantaDepasire)
            {
                gameObject.SetActive(false);
            }
        }

        // 3. Animația (dacă are animator)
        if (animator != null)
        {
            animator.Play("Run");
        }
    }

    // Funcție apelată de GameDirector sau PlayerController când se resetează mapa
    public void ResetBot()
    {
        transform.position = pozitieStart;
        gameObject.SetActive(true);
    }
}