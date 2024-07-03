# Charts
 Charts are manifest files that terraform in yaml. Terraform decodes these yaml files to terraform code this is done in the `manifest.tf`. 
 The namespace file is important that the name of the file ends in namespace.yaml. This is because it is used by different files. Namespace file has to created before the other ones because they the dependent on a namespace. 
 because of this the namespace file is decoded outside of the foreach loop in the terraform manifest. 
 
 Because the different work environment dev and prod the charts are split in two to match and also in frontend og backend. 
 Manifest also be found by using this command: 
 ` kubectl get <the manitfest>  -n <namespace> -o yaml`
 This outputs the file in yaml in the terminal. 
 > if you need more on how to get aws access for kubectl look in the terraform readme.
