using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetCannabis
{
    public class HistoriqueModifications
    {

        public int Id { get; set; }
        public string Identification { get; set; }
        public string Description { get; set; }
        public DateTime DateModification { get; set; }
        public string Provenance { get; set; }
        public string EtatSante { get; set; }
        public string StadeVie { get; set; }
        public int ActifInactif { get; set; }
        public string Entreposage { get; set; }
        public string ItemRetireInventaire { get; set; }
        public string ResponsableDecontamination { get; set; }
        public string Note { get; set; }
    }
}
