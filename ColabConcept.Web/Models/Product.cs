using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColabConcept.Web.Models
{
    public class Product
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid Version
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string LockedBy
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            Product other = obj as Product;
            
            if (other == null)
                return false;

            if (other.Id == Guid.Empty || this.Id == Guid.Empty)
                return false;

            return other.Id == this.Id;
        }



        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}