using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnrmVaarRaport
{
    internal class TypeInzet : BaseData
    {
        internal SortedDictionary<string, BaseData> SdBoot { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdWeer { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdWindkracht { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdAndereHulpverleners { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdBehoevenVan { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdVaartuiggroep { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdOorzaak { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdPositie { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdPrio { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdWindrichting { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdZicht { get; private set; } = new();
        internal SortedDictionary<string, BaseData> SdOproepGedaanDoor { get; private set; } = new();
        internal int AantalGeredden { get; private set; } = 0;
        internal int AantalDieren { get; private set; } = 0;
        internal int AantalOpvarende { get; private set; } = 0;
        internal int Aantaloverledenen { get; private set; } = 0;
        internal TypeInzet() : base()
        {
            SdBoot = new SortedDictionary<string, BaseData>();
        }
        internal void AddData(double hours, BaseData boot, string weer, string windkracht, string[] andereHulpverleners, int aantalGeredden, int aantalDieren, int aantalOpvarende, int aantaloverledenen, string behoevenVan, string vaartuiggroep, string oorzaken, string positie, string prio, string windrichting, string zicht, string oproepGedaanDoor)
        {
            UpdateData(SdBoot, boot.Name, hours);
            UpdateData(SdWeer, weer, hours);
            UpdateData(SdWindkracht, windkracht, hours);
            UpdateData(SdAndereHulpverleners, andereHulpverleners, hours);
            UpdateData(SdBehoevenVan, behoevenVan, hours);
            UpdateData(SdVaartuiggroep, vaartuiggroep, hours);
            UpdateData(SdOorzaak, oorzaken, hours);
            UpdateData(SdPositie, positie, hours);
            UpdateData(SdPrio, prio, hours);
            UpdateData(SdWindrichting, windrichting, hours);
            UpdateData(SdZicht, zicht, hours);
            UpdateData(SdOproepGedaanDoor, oproepGedaanDoor, hours);

            AantalGeredden += aantalGeredden;
            AantalDieren += aantalDieren;
            AantalOpvarende += aantalOpvarende;
            Aantaloverledenen += aantaloverledenen;
        }

        private void UpdateData(SortedDictionary<string, BaseData> dataObjects, string data, double hours)
        {
            if (!dataObjects.ContainsKey(data))
                dataObjects.Add(data, new BaseData() { Name = data });
            dataObjects.TryGetValue(data, out var dataObject);
            dataObject.Count++;
            dataObject.SetHours(hours);
        }

        private void UpdateData(SortedDictionary<string, BaseData> dataObject, string[] datas, double hours)
        {
            foreach (var data in datas)
                UpdateData(dataObject, data, hours);
        }
    }
}
