namespace TaskInitializer.Models.Common
{
    public class StoredProcedure
    {
        public const string ChannelScheduleSelect = "SELECT (SELECT CURRENT_TIME ##TIMEZONEOFFSET## hours'::INTERVAL) BETWEEN TO_TIMESTAMP(CONCAT((SELECT PR.propertyvalue FROM property PR WHERE PR.corpid = {0} AND PR.orgid = {1} AND PR.communicationchannelid = {2} AND PR.propertyname = 'HORAINIATENCION' AND PR.status = 'ACTIVO')), 'HH24:MI')::TIME AND TO_TIMESTAMP(CONCAT((SELECT PR.propertyvalue FROM property PR WHERE PR.corpid = {3} AND PR.orgid = {4} AND PR.communicationchannelid = {5} AND PR.propertyname = 'HORAFINATENCION' AND PR.status = 'ACTIVO')), 'HH24:MI')::TIME AS atencion;";

        public const string CallConversationSelect = "SELECT CO.conversationid, CO.corpid, CO.orgid FROM conversation CO LEFT JOIN personcommunicationchannel PCC ON CO.personcommunicationchannel = PCC.personcommunicationchannel AND CO.corpid = PCC.corpid AND CO.orgid = PCC.orgid AND PCC.displayname ILIKE {0} WHERE CO.personcommunicationchannel ILIKE '%CALL%' ORDER BY CO.conversationid DESC LIMIT 1;";

        public const string CleanTicketSelect = "SELECT CO.corpid, CO.orgid, CO.personcommunicationchannel, CO.ticketnum, CO.conversationid, CC.communicationchannelsite, CC.type FROM conversation CO INNER JOIN communicationchannel CC ON CO.communicationchannelid = CC.communicationchannelid WHERE";

        public const string CallInteractionSelect = "SELECT corpid, orgid, personid, conversationid, interactionid, interactiontext FROM interaction IT WHERE IT.conversationid = {0} AND IT.corpid = {1} AND IT.orgid = {2} AND interactiontext = '<GRABACIÓN DE LA LLAMADA>' ORDER BY interactionid ASC LIMIT 1;";

        public const string CheckHoldingOrganizationSelect = "SELECT CO.corpid, OG.orgid, CO.description AS corpdescription, OG.description AS orgdescription FROM org OG LEFT JOIN corp CO ON OG.corpid = CO.corpid WHERE OG.status = 'ACTIVO' ORDER BY CO.corpid, OG.orgid;";

        public const string SessionCheckSelect = "SELECT pid, datname, usename, query_start, query, state FROM pg_stat_activity WHERE (state = 'active' OR state = 'idle') AND datname = 'ZYXMEDBCLAROPRD' ORDER BY state, query_start;";

        public const string CallInteractionUpdate = "UPDATE interaction SET interactiontext = {0}, interactiontype = 'audio' WHERE corpid = {1} AND orgid = {2} AND personid = {3} AND conversationid = {4} AND interactionid = {5};";

        public const string ReportTemplateSelect = "SELECT columnjson FROM reporttemplate WHERE reporttemplateid = {0} AND corpid = {1} AND orgid = {2};";

        public const string MailAlertUpdate = "UPDATE conversation SET emailalertsent = true WHERE conversationid = {0};";

        public const string ConversationOverviewSelect = "SELECT * FROM ufn_report_conversation_overview({0}, {1});";

        public const string ActiveBillingSelect = "SELECT org.corpid, org.orgid, corp.billbyorg FROM org LEFT JOIN corp ON org.corpid = corp.corpid WHERE org.status = 'ACTIVO' AND corp.status = 'ACTIVO' ORDER BY 1, 2;";

        public const string ActiveOrganizationSelect = "SELECT corpid, orgid FROM org WHERE status = 'ACTIVO';";

        public const string UserProductivitySelect = "SELECT * FROM ufn_userproductivity_sel({0}, {1}, {2});";

        public const string UpdateBillingMonthSelect = "SELECT * FROM ufn_billingconfiguration_newmonth({0}, {1});";

        public const string UpdateBillingSelect = "SELECT * FROM ufn_billingperiod_calc({0}, {1}, {2}, {3}, {4});";

        public const string UpdateHsmSelect = "SELECT * FROM ufn_billingperiodhsm_calc({0}, {1}, {2}, {3}, {4});";

        public const string SelectActiveChannel = "SELECT CC.communicationchannelsite, CC.schedule, CC.status, CC.type, CC.communicationchannelid, CC.corpid, CC.orgid, CC.channelactive, OG.timezoneoffset FROM communicationchannel CC LEFT JOIN org OG ON CC.orgid = OG.orgid WHERE CC.status = 'ACTIVO' and OG.status = 'ACTIVO';";

        public const string UpdateActiveChannel = "UPDATE communicationchannel SET channelactive = {0} WHERE corpid = {1} AND orgid = {2} AND communicationchannelid = {3};";

        public const string SecurityValidationSelect = "SELECT * FROM ufn_cron_securityvalidation_sel();";

        public const string CleanSmoochSessionSelect = "SELECT * FROM ufn_cron_smoochsesion_clean({0});";

        public const string HoldingCountSelect = "SELECT * from ufn_generic_holdingcount_sel({0}, {1});";

        public const string CampaignCheckSelect = "SELECT * FROM ufn_campaign_check({0}, {1}, {2});";

        public const string MailAlertSelect = "SELECT * FROM ufn_cron_emailalert_sel({0}, {1});";

        public const string AbandonedTicketSelect = "SELECT * FROM ufn_cron_abandoned_sel();";

        public const string SessionExpiredSelect = "SELECT * FROM ufn_usersession_expired();";

        public const string SelectInvoiceOrg = "SELECT * FROM ufn_invoice_org({0}, {1}, {2}, {3}, {4});";

        public const string SelectInvoiceCorp = "SELECT * FROM ufn_invoice_corp({0}, {1}, {2}, {3});";

        public const string UpdateInvoice = "SELECT * FROM ufn_invoice_sunat({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9});";

        public const string SelectInvoiceCorrelative = "SELECT * FROM ufn_invoice_correlative({0}, {1}, {2});";

        public const string SelectKpiRun = "SELECT * FROM ufn_kpi_run({0});";

        public const string SelectKpiCalc = "SELECT * FROM ufn_kpi_calc({0}, {1}, {2}, {3}, {4});";

        public const string InsertHsmHistory = "INSERT INTO hsmhistory (corpid, orgid, description, status, type, createdate, createby, changedate, changeby, success, message, config, shippingreason, messagetemplateid) VALUES ({0}, {1}, '', 'FINALIZADO', 'MAIL', NOW(), 'admin', NOW(), 'admin', {2}, {3}, {4}, {5}, {6});";

        public const string SelectReportSchedulerRun = "SELECT * FROM ufn_reportscheduler_run({0});";

        public const string SelectReportSchedulerCalc = "SELECT * FROM ufn_reportscheduler_calc({0}, {1}, {2}, {3}, {4});";

        public const string SelectAutomaticCampaign = "SELECT MO.name, MR.orgid, MR.corpid, MR.conversationid FROM mro_order MO INNER JOIN mro_request MR ON MO.request_id = MR.id WHERE MO.name = {0} ORDER BY MO.name;";

        public const string SelectLaraigoConversation = "SELECT communicationchannelid, personid FROM conversation WHERE corpid = {0} AND orgid = {1} AND conversationid = {2};";

        public const string SelectLaraigoPerson = "SELECT firstname, lastname, phone FROM person WHERE corpid = {0} AND orgid = {1} AND personid = {2};";

        public const string SelectVoximplantRecharge = "SELECT OG.corpid, OG.orgid, OG.timezoneoffset, OG.voximplantautomaticrecharge, OG.voximplantrechargerange, OG.voximplantrechargepercentage, OG.voximplantrechargefixed, OG.voximplantrechargerundate FROM org OG LEFT JOIN corp CO ON OG.corpid = CO.corpid WHERE OG.status = 'ACTIVO' AND CO.status = 'ACTIVO' AND voximplantautomaticrecharge = true AND (voximplantrechargerundate IS NULL OR (voximplantrechargerundate + (COALESCE(OG.timezoneoffset, 0) * INTERVAL '1HOUR'))::date < (NOW() + (COALESCE(OG.timezoneoffset, 0) * INTERVAL '1HOUR'))::date);";

        public const string SelectVoximplantUpdate = "SELECT * FROM ufn_org_voximplant_recharge({0}, {1});";

        public const string ActiveVoximplantSelect = "SELECT org.corpid, org.orgid, corp.billbyorg FROM org LEFT JOIN corp ON org.corpid = corp.corpid WHERE org.status = 'ACTIVO' AND corp.status = 'ACTIVO' AND org.voximplantaccountid IS NOT NULL ORDER BY 1, 2;";

        public const string SessionCheckGeneral = "SELECT pid, datname, usename, query_start, NOW() as current_time, (NOW() - query_start) as query_time, query, state FROM pg_stat_activity WHERE (state = 'active' OR state = 'idle') AND datname = {0} ORDER BY state, query_start;";

        public const string SelectOdooProduct = "select sr.name solicitud,ot.name orden,ot.description orden_descripcion,ot.state estado,eq.equipment_number placa,(sr.requested_date - interval '5 hour')::text fecha_solicitud,(ot.date_start_execution - interval '5 hour')::text fecha_inicio_real,(ot.date_execution)::text fecha_fin_real,mt.name distribuidor,rp.vat idcliente,rp.name cliente,pt.name producto,otp.quantity cantidad,f1.name problema,f2.name causa,f3.name solución from mro_order_product_line otp left join mro_order ot on ot.id = otp.order_id left join product_product prd on otp.product_id = prd.id left join product_template pt on pt.id = prd.product_tmpl_id left join mro_request sr on ot.request_id = sr.id left join mro_equipment eq on ot.equipment_id = eq.id left join mro_maintenance_team mt on mt.id = ot.dt_id left join res_partner rp on rp.id = sr.customer_id left join mro_failures f1 on f1.id = ot.problem_id left join mro_failures f2 on f2.id = ot.cause_id left join mro_failures f3 on f3.id = ot.solution_id;";

        public const string SelectOdooEquipment = "select sr.name solicitud,case when sr.maintenance_priority = '0' then 'Bajo'when sr.maintenance_priority = '1' then 'Normal'when sr.maintenance_priority = '2' then 'Alto'when sr.maintenance_priority = '3' then 'Averiado'else 'xxx' end prioridad,ot.name orden,sr.cause tipo_problema,ot.state estado,eq.equipment_number placa,(sr.requested_date - interval '5 hour')::text fecha_solicitud,(ot.date_start_execution - interval '5 hour')::text fecha_inicio_real,(ot.date_execution)::text fecha_fin_real,mt.name distribuidor,rp.vat idcliente,rp.name cliente,rp.ruc ruc,rp.dni dni from mro_request sr left join mro_order ot on ot.request_id = sr.id left join mro_equipment eq on ot.equipment_id = eq.id left join mro_maintenance_team mt on mt.id = ot.dt_id left join res_partner rp on rp.id = sr.customer_id;";

        public const string SelectSunatInteraction = "SELECT * FROM ufn_sunat_interaction_export_attachment({0}, {1}, {2}, {3});";

        public const string SelectProviderList = "select id, email_notification, name from mro_maintenance_team where type = 'distribuidor';";

        public const string SelectProviderReport = "select mtd.name distribuidor, r.name solicitud_mantenimiento, o.name numero_orden, e.equipment_number codigo_equipo, e.name descripcion_equipo, p.vat clave_cliente, p.name nombre_cliente, r.address direccion, case o.maintenance_type when 'bm' then 'Correctivo' when 'pm' then 'Preventivo' when 'oc' then 'Por condición' when 'pr' then 'Periodico' when 'in' then 'Inspección' when 'rf' then 'Reacondicionamiento' when 'mp' then 'Plan de mantenimiento' else 'xxx' end tipo_mantenimiento, mts.name equipo_mantenimiento, pu.name tecnico_responsable, o.date_planned fecha_solicitada, o.date_execution fecha_finalización_ejecución, case o.state when 'draft' then 'Borrador' when 'released' then 'Confirmado' when 'done' then 'Cerrado' when 'cancel' then 'Rechazado' else 'xxx' end estado, o.cause causa_reportada, o.description descripcion, case o.product_replenishment when '1' then 'Procede' when '2' then 'Procede Parcialmente' when '3' then 'No Procede' else 'xxx' end reposicion_productos, pt.name descripcion_producto, op.quantity cantidad, pt.list_price costo, cast(coalesce(replace(fc.description,'%',''),'0') as numeric) porcentaje_reconocimiento, (pt.list_price * op.quantity * cast(coalesce(replace(fc.description,'%',''),'0') as numeric))/100 subtotal, fc.name causa, fp.name problema from mro_order o  inner join mro_order_product_line op on o.id = op.order_id left join mro_request r on o.request_id = r.id left join mro_maintenance_team mtd on r.dt_id = mtd.id left join mro_equipment e on o.equipment_id = e.id left join res_partner p on r.customer_id = p.id left join mro_maintenance_team mts on o.maintenance_team_id = mts.id left join res_users u on o.user_id = u.id left join res_partner pu on u.partner_id = pu.id left join mro_failures fc on o.cause_id = fc.id left join mro_failures fp on o.problem_id = fp.id left join product_product pp on op.product_id = pp.id left join product_template pt on pp.product_tmpl_id = pt.id where o.state = 'done' and r.dt_id = {0};";

        public const string SelectWitaiCron = "SELECT * FROM ufn_witai_app_cron();";

        public const string SelectWitaiConfig = "SELECT * FROM ufn_witai_app_config({0}, {1}, {2}, {3}, {4});";

        public const string SelectWitaiSchedule = "SELECT * FROM ufn_witai_worker_scheduled_sel();";

        public const string SelectWitaiWorker = "SELECT * FROM ufn_witai_worker_status_upd({0}, {1}, {2}, {3});";
    }
}