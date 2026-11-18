namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
 
    // - profilu sarakstā (login ekrānā)
    // - saglabasanai users.json failā
    // - lietotāja identifikācijai aplikācijā
    public class UserEntry
    {

        public int Id { get; set; }

        // Lietotājvārds (redzams UI un izmantots mapju nosaukumos)
        public string Name { get; set; } = "";


        public string Password { get; set; } = "";
    }
}
