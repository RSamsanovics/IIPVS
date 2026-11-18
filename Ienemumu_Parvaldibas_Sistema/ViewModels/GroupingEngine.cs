using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ienemumu_Parvaldibas_Sistema.AppModels;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
    // Statiskaa klase, kas atbild par darijuma piesaisti grupai
    internal static class GroupingEngine
    {
        // Nosaka grupas nosaukumu konkrētam darījumam
        // partner – teksts no darījuma ( Partner / apraksts)
        // groups  – lietotāja def grupas
        public static string ResolveGroup(
            string partner,
            IEnumerable<GroupDefinition> groups)
        {
            // Atrod visas aktivaas grupas, kuras aktualas
            var matched = groups
                //  tikai ieslegtas grupas
                .Where(g => g.IsEnabled)
                // parbaude vai vismaz viens atslegvards sakrīt 
                .Where(g =>
                    g.Keywords.Any(k =>
                        partner.Contains(k, StringComparison.OrdinalIgnoreCase)))
                // Panem  grupu nosaukumus
                .Select(g => g.Name)
                // Noņem dublikatus 
                .Distinct()
                .ToList();

            // Ja nav atrasta neviena grupa = "Other"
            if (matched.Count == 0)
            {
                return "Other";
            }

            // Ja līdz 3 grupam = custom grupa
            if (matched.Count <= 3)
            {
                return string.Join(" + ", matched);
            }

            // Ja grupu ir par daudz,
            return "Other";
        }
    }
}
