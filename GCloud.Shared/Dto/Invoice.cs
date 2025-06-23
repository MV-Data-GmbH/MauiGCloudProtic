

namespace GCloud.Shared.Dto
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    //// [System.SerializableAttribute()]
    //// [System.ComponentModel.DesignerCategoryAttribute("code")]
    //// [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.ebinterface.at/schema/4p3/", IsNullable = false)]
    public class Invoice
    {
        private string invoiceNumberField;

        private System.DateTime invoiceDateField;

        private InvoiceDelivery deliveryField;

        private InvoiceBiller billerField;

        private InvoiceInvoiceRecipient invoiceRecipientField;

        private InvoiceDetails detailsField;

        private InvoiceTax taxField;

        private decimal totalGrossAmountField;

        private decimal payableAmountField;

        private List<InvoicePaymentMethod> paymentMethodsField = new List<InvoicePaymentMethod>();

        private InvoicePaymentConditions paymentConditionsField;

        private string commentField;

        private string languageField;

        private string invoiceCurrencyField;

        private string documentTypeField;

        private string generatingSystemField;

        private string jwsSignatureField;

        /// <remarks/>
        public string InvoiceNumber
        {
            get { return this.invoiceNumberField; }
            set { this.invoiceNumberField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime InvoiceDate
        {
            get { return this.invoiceDateField; }
            set { this.invoiceDateField = value; }
        }

        /// <remarks/>
        public InvoiceDelivery Delivery
        {
            get { return this.deliveryField; }
            set { this.deliveryField = value; }
        }

        /// <remarks/>
        public InvoiceBiller Biller
        {
            get { return this.billerField; }
            set { this.billerField = value; }
        }

        /// <remarks/>
        public InvoiceInvoiceRecipient InvoiceRecipient
        {
            get { return this.invoiceRecipientField; }
            set { this.invoiceRecipientField = value; }
        }

        /// <remarks/>
        public InvoiceDetails Details
        {
            get { return this.detailsField; }
            set { this.detailsField = value; }
        }

        /// <remarks/>
        public InvoiceTax Tax
        {
            get { return this.taxField; }
            set { this.taxField = value; }
        }

        /// <remarks/>
        public decimal TotalGrossAmount
        {
            get { return this.totalGrossAmountField; }
            set { this.totalGrossAmountField = value; }
        }

        /// <remarks/>
        public decimal PayableAmount
        {
            get { return this.payableAmountField; }
            set { this.payableAmountField = value; }
        }

        /// <remarks/>
        public List<InvoicePaymentMethod> PaymentMethods
        {
            get { return this.paymentMethodsField; }
            set { this.paymentMethodsField = value; }
        }

        /// <remarks/>
        public InvoicePaymentConditions PaymentConditions
        {
            get { return this.paymentConditionsField; }
            set { this.paymentConditionsField = value; }
        }

        /// <remarks/>
        public string Comment
        {
            get { return this.commentField; }
            set { this.commentField = value; }
        }

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string Language
        {
            get { return this.languageField; }
            set { this.languageField = value; }
        }

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string InvoiceCurrency
        {
            get { return this.invoiceCurrencyField; }
            set { this.invoiceCurrencyField = value; }
        }

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string DocumentType
        {
            get { return this.documentTypeField; }
            set { this.documentTypeField = value; }
        }

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string GeneratingSystem
        {
            get { return this.generatingSystemField; }
            set { this.generatingSystemField = value; }
        }

        public string JwsSignature
        {
            get { return this.jwsSignatureField; }
            set { this.jwsSignatureField = value; }
        }

        public string Tag { get; set; }
    }

    /// <remarks/>
    //// [System.SerializableAttribute()]
    //// [System.ComponentModel.DesignerCategoryAttribute("code")]
    //// [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDelivery
    {
        private System.DateTime dateField;

        private InvoiceDeliveryAddress addressField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime Date
        {
            get { return this.dateField; }
            set { this.dateField = value; }
        }

        /// <remarks/>
        public InvoiceDeliveryAddress Address
        {
            get { return this.addressField; }
            set { this.addressField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDeliveryAddress
    {
        private string salutationField;

        private string nameField;

        private string streetField;

        private string townField;

        private ushort zIPField;

        private InvoiceDeliveryAddressCountry countryField;

        private string contactField;

        /// <remarks/>
        public string Salutation
        {
            get { return this.salutationField; }
            set { this.salutationField = value; }
        }

        /// <remarks/>
        public string Name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <remarks/>
        public string Street
        {
            get { return this.streetField; }
            set { this.streetField = value; }
        }

        /// <remarks/>
        public string Town
        {
            get { return this.townField; }
            set { this.townField = value; }
        }

        /// <remarks/>
        public ushort ZIP
        {
            get { return this.zIPField; }
            set { this.zIPField = value; }
        }

        /// <remarks/>
        public InvoiceDeliveryAddressCountry Country
        {
            get { return this.countryField; }
            set { this.countryField = value; }
        }

        /// <remarks/>
        public string Contact
        {
            get { return this.contactField; }
            set { this.contactField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDeliveryAddressCountry
    {
        private string countryCodeField;

        private string valueField;

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string CountryCode
        {
            get { return this.countryCodeField; }
            set { this.countryCodeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceBiller
    {
        private string vATIdentificationNumberField;

        private InvoiceBillerAddress addressField;

        private uint invoiceRecipientsBillerIDField;

        private string companyNameField;

        /// <remarks/>
        public string VATIdentificationNumber
        {
            get { return this.vATIdentificationNumberField; }
            set { this.vATIdentificationNumberField = value; }
        }

        /// <remarks/>
        public InvoiceBillerAddress Address
        {
            get { return this.addressField; }
            set { this.addressField = value; }
        }

        /// <remarks/>
        public uint InvoiceRecipientsBillerID
        {
            get { return this.invoiceRecipientsBillerIDField; }
            set { this.invoiceRecipientsBillerIDField = value; }
        }

        /// <remarks/>
        public string ComanyName
        {
            get { return this.companyNameField; }
            set { this.companyNameField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceBillerAddress
    {
        private string salutationField;

        private string nameField;

        private string streetField;

        private string townField;

        private ushort zIPField;

        private InvoiceBillerAddressCountry countryField;

        private string emailField;

        private string contactField;

        /// <remarks/>
        public string Salutation
        {
            get { return this.salutationField; }
            set { this.salutationField = value; }
        }

        /// <remarks/>
        public string Name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <remarks/>
        public string Street
        {
            get { return this.streetField; }
            set { this.streetField = value; }
        }

        /// <remarks/>
        public string Town
        {
            get { return this.townField; }
            set { this.townField = value; }
        }

        /// <remarks/>
        public ushort ZIP
        {
            get { return this.zIPField; }
            set { this.zIPField = value; }
        }

        /// <remarks/>
        public InvoiceBillerAddressCountry Country
        {
            get { return this.countryField; }
            set { this.countryField = value; }
        }

        /// <remarks/>
        public string Email
        {
            get { return this.emailField; }
            set { this.emailField = value; }
        }

        /// <remarks/>
        public string Contact
        {
            get { return this.contactField; }
            set { this.contactField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceBillerAddressCountry
    {
        private string countryCodeField;

        private string valueField;

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string CountryCode
        {
            get { return this.countryCodeField; }
            set { this.countryCodeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceInvoiceRecipient
    {
        private string vATIdentificationNumberField;

        private InvoiceInvoiceRecipientFurtherIdentification[] furtherIdentificationField;

        private InvoiceInvoiceRecipientOrderReference orderReferenceField;

        private InvoiceInvoiceRecipientAddress addressField;

        /// <remarks/>
        public string VATIdentificationNumber
        {
            get { return this.vATIdentificationNumberField; }
            set { this.vATIdentificationNumberField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FurtherIdentification")]
        public InvoiceInvoiceRecipientFurtherIdentification[] FurtherIdentification
        {
            get { return this.furtherIdentificationField; }
            set { this.furtherIdentificationField = value; }
        }

        /// <remarks/>
        public InvoiceInvoiceRecipientOrderReference OrderReference
        {
            get { return this.orderReferenceField; }
            set { this.orderReferenceField = value; }
        }

        /// <remarks/>
        public InvoiceInvoiceRecipientAddress Address
        {
            get { return this.addressField; }
            set { this.addressField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceInvoiceRecipientFurtherIdentification
    {
        private string identificationTypeField;

        private string valueField;

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string IdentificationType
        {
            get { return this.identificationTypeField; }
            set { this.identificationTypeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceInvoiceRecipientOrderReference
    {
        private string orderIDField;

        private System.DateTime referenceDateField;

        private string descriptionField;

        /// <remarks/>
        public string OrderID
        {
            get { return this.orderIDField; }
            set { this.orderIDField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime ReferenceDate
        {
            get { return this.referenceDateField; }
            set { this.referenceDateField = value; }
        }

        /// <remarks/>
        public string Description
        {
            get { return this.descriptionField; }
            set { this.descriptionField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceInvoiceRecipientAddress
    {
        private string nameField;

        private string streetField;

        private string townField;

        private ushort zIPField;

        private InvoiceInvoiceRecipientAddressCountry countryField;

        private string phoneField;

        private string emailField;

        private string contactField;

        /// <remarks/>
        public string Name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <remarks/>
        public string Street
        {
            get { return this.streetField; }
            set { this.streetField = value; }
        }

        /// <remarks/>
        public string Town
        {
            get { return this.townField; }
            set { this.townField = value; }
        }

        /// <remarks/>
        public ushort ZIP
        {
            get { return this.zIPField; }
            set { this.zIPField = value; }
        }

        /// <remarks/>
        public InvoiceInvoiceRecipientAddressCountry Country
        {
            get { return this.countryField; }
            set { this.countryField = value; }
        }

        /// <remarks/>
        public string Phone
        {
            get { return this.phoneField; }
            set { this.phoneField = value; }
        }

        /// <remarks/>
        public string Email
        {
            get { return this.emailField; }
            set { this.emailField = value; }
        }

        /// <remarks/>
        public string Contact
        {
            get { return this.contactField; }
            set { this.contactField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceInvoiceRecipientAddressCountry
    {
        private string countryCodeField;

        private string valueField;

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string CountryCode
        {
            get { return this.countryCodeField; }
            set { this.countryCodeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDetails
    {
        private string headerDescriptionField;

        private InvoiceDetailsItemList itemListField;

        private string footerDescriptionField;

        /// <remarks/>
        public string HeaderDescription
        {
            get { return this.headerDescriptionField; }
            set { this.headerDescriptionField = value; }
        }

        /// <remarks/>
        public InvoiceDetailsItemList ItemList
        {
            get { return this.itemListField; }
            set { this.itemListField = value; }
        }

        /// <remarks/>
        public string FooterDescription
        {
            get { return this.footerDescriptionField; }
            set { this.footerDescriptionField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDetailsItemList
    {
        private string headerDescriptionField;

        private InvoiceDetailsItemListListLineItem[] listLineItemField;

        private string footerDescriptionField;

        /// <remarks/>
        public string HeaderDescription
        {
            get { return this.headerDescriptionField; }
            set { this.headerDescriptionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ListLineItem")]
        public InvoiceDetailsItemListListLineItem[] ListLineItem
        {
            get { return this.listLineItemField; }
            set { this.listLineItemField = value; }
        }

        /// <remarks/>
        public string FooterDescription
        {
            get { return this.footerDescriptionField; }
            set { this.footerDescriptionField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDetailsItemListListLineItem
    {
        private string descriptionField;

        private InvoiceDetailsItemListListLineItemQuantity quantityField;

        private decimal unitPriceField;

        private byte vATRateField;

        private InvoiceDetailsItemListListLineItemInvoiceRecipientsOrderReference invoiceRecipientsOrderReferenceField;

        private decimal lineItemAmountField;

        /// <remarks/>
        public string Description
        {
            get { return this.descriptionField; }
            set { this.descriptionField = value; }
        }

        /// <remarks/>
        public InvoiceDetailsItemListListLineItemQuantity Quantity
        {
            get { return this.quantityField; }
            set { this.quantityField = value; }
        }

        /// <remarks/>
        public decimal UnitPrice
        {
            get { return this.unitPriceField; }
            set { this.unitPriceField = value; }
        }

        /// <remarks/>
        public byte VATRate
        {
            get { return this.vATRateField; }
            set { this.vATRateField = value; }
        }

        /// <remarks/>
        public InvoiceDetailsItemListListLineItemInvoiceRecipientsOrderReference InvoiceRecipientsOrderReference
        {
            get { return this.invoiceRecipientsOrderReferenceField; }
            set { this.invoiceRecipientsOrderReferenceField = value; }
        }

        /// <remarks/>
        public decimal LineItemAmount
        {
            get { return this.lineItemAmountField; }
            set { this.lineItemAmountField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDetailsItemListListLineItemQuantity
    {
        private string unitField;

        private ushort valueField;

        /// <remarks/>
        // [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string Unit
        {
            get { return this.unitField; }
            set { this.unitField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ushort Value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceDetailsItemListListLineItemInvoiceRecipientsOrderReference
    {
        private string orderIDField;

        private byte orderPositionNumberField;

        /// <remarks/>
        public string OrderID
        {
            get { return this.orderIDField; }
            set { this.orderIDField = value; }
        }

        /// <remarks/>
        public byte OrderPositionNumber
        {
            get { return this.orderPositionNumberField; }
            set { this.orderPositionNumberField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceTax
    {
        private InvoiceTaxVATItem[] vATField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("VATItem", IsNullable = false)]
        public InvoiceTaxVATItem[] VAT
        {
            get { return this.vATField; }
            set { this.vATField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoiceTaxVATItem
    {
        private decimal taxedAmountField;

        private decimal vATRateField;

        private decimal amountField;

        /// <remarks/>
        public decimal TaxedAmount
        {
            get { return this.taxedAmountField; }
            set { this.taxedAmountField = value; }
        }

        /// <remarks/>
        public decimal VATRate
        {
            get { return this.vATRateField; }
            set { this.vATRateField = value; }
        }

        /// <remarks/>
        public decimal Amount
        {
            get { return this.amountField; }
            set { this.amountField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoicePaymentMethod
    {
        private decimal amountField;

        private string commentField;

        private InvoicePaymentMethodUniversalBankTransaction universalBankTransactionField;

        /// <remarks/>
        public string Comment
        {
            get { return this.commentField; }
            set { this.commentField = value; }
        }

        /// <remarks/>
        public decimal Amount
        {
            get { return this.amountField; }
            set { this.amountField = value; }
        }

        /// <remarks/>
        public InvoicePaymentMethodUniversalBankTransaction UniversalBankTransaction
        {
            get { return this.universalBankTransactionField; }
            set { this.universalBankTransactionField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoicePaymentMethodUniversalBankTransaction
    {
        private InvoicePaymentMethodUniversalBankTransactionBeneficiaryAccount beneficiaryAccountField;

        /// <remarks/>
        public InvoicePaymentMethodUniversalBankTransactionBeneficiaryAccount BeneficiaryAccount
        {
            get { return this.beneficiaryAccountField; }
            set { this.beneficiaryAccountField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoicePaymentMethodUniversalBankTransactionBeneficiaryAccount
    {
        private string bICField;

        private string iBANField;

        private string bankAccountOwnerField;

        /// <remarks/>
        public string BIC
        {
            get { return this.bICField; }
            set { this.bICField = value; }
        }

        /// <remarks/>
        public string IBAN
        {
            get { return this.iBANField; }
            set { this.iBANField = value; }
        }

        /// <remarks/>
        public string BankAccountOwner
        {
            get { return this.bankAccountOwnerField; }
            set { this.bankAccountOwnerField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoicePaymentConditions
    {
        private System.DateTime dueDateField;

        private InvoicePaymentConditionsDiscount[] discountField;

        private string commentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime DueDate
        {
            get { return this.dueDateField; }
            set { this.dueDateField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Discount")]
        public InvoicePaymentConditionsDiscount[] Discount
        {
            get { return this.discountField; }
            set { this.discountField = value; }
        }

        /// <remarks/>
        public string Comment
        {
            get { return this.commentField; }
            set { this.commentField = value; }
        }
    }

    /// <remarks/>
    // [System.SerializableAttribute()]
    // [System.ComponentModel.DesignerCategoryAttribute("code")]
    // [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ebinterface.at/schema/4p3/")]
    public partial class InvoicePaymentConditionsDiscount
    {
        private System.DateTime paymentDateField;

        private decimal percentageField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime PaymentDate
        {
            get { return this.paymentDateField; }
            set { this.paymentDateField = value; }
        }

        /// <remarks/>
        public decimal Percentage
        {
            get { return this.percentageField; }
            set { this.percentageField = value; }
        }
    }
}