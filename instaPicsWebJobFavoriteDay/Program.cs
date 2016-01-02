using instaPicsWebJobFavoriteDay.Model;
using instaPicsWebJobFavoriteDay.Model.POCO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace instaPicsWebJobFavoriteDay
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string connectionString = CloudConfigurationManager.GetSetting(Constants.ConnStringKey);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //récupération des enregistrements de user/img
            CloudTable tableUserImg = tableClient.GetTableReference(CloudConfigurationManager.GetSetting(Constants.TableUserImgStringKey));
            tableUserImg.CreateIfNotExists();

            TableQuery<UserImageEntity> queryUserImg = new TableQuery<UserImageEntity>();
            IEnumerable<UserImageEntity> lstTableUserImage = tableUserImg.ExecuteQuery(queryUserImg);

            //récupération des enregistrements de imgfavorite
            CloudTable tableImgFavorite = tableClient.GetTableReference(CloudConfigurationManager.GetSetting(Constants.TableImgFavStringKey));
            tableImgFavorite.CreateIfNotExists();

            TableQuery<ImgFavEntity> queryImgFavorite = new TableQuery<ImgFavEntity>();
            IEnumerable<ImgFavEntity> lstTableImgFavorite = tableImgFavorite.ExecuteQuery(queryImgFavorite);

            //récupération du blobcontainer imgblob
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting(Constants.BlobImgConvertStringKey));
            if (container.CreateIfNotExists())
            {
                container.SetPermissions(new BlobContainerPermissions()
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            }

            //récupération des blobs
            IEnumerable<CloudBlockBlob> listblobs = container.ListBlobs().OfType<CloudBlockBlob>();

            //pour chaque enregistrement et la tale userImg
            foreach (UserImageEntity userImg in lstTableUserImage)
            {
                bool find = false;
                //on parcour la table ImgFav pour savoir si l'image a été mise en favoris
                foreach(ImgFavEntity imgFav in lstTableImgFavorite)
                {
                    if(userImg.imgBNThumb == imgFav.NameImage)
                    {
                        find = true;
                        break;
                    }
                }
                //si limage n'est pas en favoris, on supprime les références des images dans la table userImg et dans le blobContainer
                if(find == false)
                {
                    foreach (CloudBlockBlob myblob in listblobs)
                    {
                        if (myblob.Name == userImg.imgOriginal || myblob.Name == userImg.imgBN || myblob.Name == userImg.imgOriginalThumb || myblob.Name == userImg.imgBNThumb)
                        {
                            myblob.Delete();
                        }
                    }
                    TableOperation deleteOperation = TableOperation.Delete(userImg);
                    tableUserImg.Execute(deleteOperation);
                }
            }
        }
    }
}
