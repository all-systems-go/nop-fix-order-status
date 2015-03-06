Fix Downloadable Product Order Status 
=====================================

This is a plugin for the [NopCommerce](http://www.nopcommerce.com/) e-commerce platform.
It is only applicable to NopCommerce version 3.5

A pull request on the NopCommerce 3.5 open source project duplicates the functionality of this plugin, but it is pending review
by the NopCommerce team and may or may not be integrated into a later release of 3.5 or 3.6

### The Problem
When an e-storefront sells downloadable products that require manual activation, orders containing those products are marked as "Complete".
Since the orders are marked as "Complete", store administrators are unable to tell which orders require their attention and further action.

NopCommerce automatically transitions an order to "Complete" when the following criteria are met:
- The products in the order have been paid for with a method that does not require further processing, e.g. credit card
- The products in the order do not require shipping

An example of a product that causes this issue would be a downloadable product with the activation type set to "Manual".
The product may also require an additional license file to be added to each order.

### The Solution
This plugin overrides the default behavior of processing orders by adding additional criteria for an order to be transitioned to "Complete".

If an order contains a product matching all of the following criteria, it is left in the "Processing" state.
Administrators selling downloadable products can then see which orders contain products requiring license files or manual activation.

- The product is a downloadable product
- The product's download activation type is set to "Manual"
- The product has not yet been activated

### Working With the Source Code
The source code provided here is only for the plugin itself. It has a dependency on the NopCommerce 3.5 code base.
The NopCommerce source code is available on CodePlex at https://nopcommerce.codeplex.com/ .

This plugin source code is intended to sit in a top-level plugin group folder at the same level as the NopCommerce solution file.
This organization keeps custom plugins from the same source grouped together, and avoids polluting NopCommerce's source-controlled
Plugins folder with external plugins.

To build the plugin:

1. Get the NopCommerce source code

2. Inside the `/src/` folder, create a folder to hold all plugins for this plugin group: `AllSystemsGo.Plugins`

3. Inside the `AllSystemsGo.Plugins` folder, clone this repository

The final path to this plugin's source should look something like this: `~\NopCommerce\src\AllSystemsGo.Plugins\nop-fix-order-status`.

The library references and build output of the plugin are set to look for NopCommerce 2 folder levels up.