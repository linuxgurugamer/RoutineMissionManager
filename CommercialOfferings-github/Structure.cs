using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings
{
    public class Structure
    {
        [XmlElement]
        public StructurePart Root = null;
        [XmlArray("Parts")]
        [XmlArrayItem(typeof(StructurePart), ElementName = "Part")]
        public List<StructurePart> Parts = null;

        public static Structure GetDockedStructure(Vessel vessel, Part dockedPort)
        {
            if (dockedPort == null) { return null; }

            List<Part> dockedParts = RmmUtil.GetDockedParts(vessel, dockedPort);

            // root of structure is the docked port
            Part rootPart = dockedPort;

            // seperate child parts
            dockedParts.Remove(dockedPort);
            List<Part> childParts = dockedParts;

            // Create structure 
            var structure = new Structure();
            structure.Parts = new List<StructurePart>();

            structure.Root = StructurePart.GetStructurePart(rootPart, rootPart);
            // decrement link count if root part is docked
            if (RmmUtil.IsDocked(vessel, dockedPort))
            { 
                structure.Root.LinkCount--;
            }

            structure.Parts = new List<StructurePart>();
            // add root to structure parts
            structure.Parts.Add(structure.Root);
            // add all child parts to structure parts
            foreach (Part childPart in dockedParts)
            {
                structure.Parts.Add(StructurePart.GetStructurePart(childPart, rootPart));
            }

            return structure;
        }

        public bool PartIsStructurePart(Part part)
        {
            foreach (StructurePart structurePart in Parts)
            {
                if (structurePart.Id == part.flightID) { return true; }
            }
            return false;
        }

        public bool Equal(Structure compareStructure)
        {
            if (this.Parts.Count != compareStructure.Parts.Count)
            {
                return false;
            }

            foreach (StructurePart part in Parts)
            {
                bool equalFound = true;
                foreach (StructurePart comparePart in compareStructure.Parts)
                {
                    if (part.Name == comparePart.Name &&
                        part.LinkCount == comparePart.LinkCount)
                    {
                        var difference = Vector3d.Distance(part.Position, comparePart.Position);
                        var averageDistance = (part.Position.magnitude + comparePart.Position.magnitude) / 2;
                        if (difference < 0.1 * averageDistance || difference < 0.1)
                        {
                            equalFound = true;
                            break;
                        }
                    }

                }
                
                if (!equalFound) { return false; }
            }

            return true;
        }


        public class StructurePart
        {
            [XmlElement]
            public uint Id = 0;
            [XmlElement]
            public string Name = "";
            [XmlElement]
            public int LinkCount = 0;
            [XmlIgnore]
            public Vector3d Position = new Vector3d();
            [XmlElement]
            public double PositionX
            {
                get { return Position.x; }
                set { Position.x = value; }
            }
            [XmlElement]
            public double PositionY
            {
                get { return Position.y; }
                set { Position.y = value; }
            }
            [XmlElement]
            public double PositionZ
            {
                get { return Position.z; }
                set { Position.z = value; }
            }

            public static StructurePart GetStructurePart(Part part, Part root)
            {
                StructurePart structurePart = new StructurePart
                {
                    Id = part.flightID,
                    Name = part.name,
                    LinkCount = RmmUtil.GetLinkedParts(part).Count, 
                    Position = root.transform.InverseTransformPoint(part.transform.position)
                };
                return structurePart;
            }
        }
    }
}
