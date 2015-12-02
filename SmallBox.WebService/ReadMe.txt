
chemin					Method	Utilisation																				Utilité.
file/{*filePath} 		POST 	filePath est le chemin et nom du fichier qui est uploadé, le body comprend le fichier.	Upload de fichier.
						GET 	filePath comprend le chemin complet du fichier a télécharger.							Télécharge un fichier.
folders/{*folderPath} 	GET 	folder Path est le chemin du dossier a afficher le contenu.								affiche le contenu d'un dossier.
root/ 					GET 	affiche le dossier racine.																affiche le contenu du dossier racine.
zip/{*folderPath} 		POST 	folder Path est le chemin complet du dossier a compresser et enregistrer dans Archives.	Zip un dossier et l'enregistre dans Archives.
zip/{*zipPath} 			GET 	zip Path est soit le nom du fichier dans Archives ou le chemin complet vers Archives.	Télécharge un fichier zip.