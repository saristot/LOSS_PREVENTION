/**
 * Parameters for Euclidean Distance Analysis
 * 
 * This analysis finds records most similar to a source record by calculating
 * multi-dimensional distance across selected fields.
 */
export interface Distance {
  /** ID of the source document to compare against */
  id: string
  
  /** Field name to use as the start date boundary */
  startDateField: string
  
  /** Field name to use as the end date boundary */
  endDateField: string
  
  /** Start date value (ISO string format) */
  startDate: string
  
  /** End date value (ISO string format) */
  endDate: string
  
  /** Field to use for labeling/identifying records in results (e.g., customer name, transaction ID) */
  keyField: string
  
  /** Array of field names to compare (minimum 3 required for meaningful analysis) */
  fields: string[]
}

export interface NamedField {
  name: string;
  value: any;
}

/**
 * Result from Euclidean Distance Analysis
 */
export interface DistanceResult {
  /** Document ID of the similar record */
  id: string
  
  /** Overall similarity score (0-100%) - combines field match and distance match */
  score: string
  
  /** Percentage of fields available for comparison */
  fieldMatch: string
  
  /** How close the field values are (100% = identical) */
  distanceMatch: string
  
  /** Comma-separated list of fields used in the comparison */
  fieldsUsed: string
  
  /** Key field information for identifying the record */
  keyField: {
    name: string
    value: string
  }
  
  /** Field values that were compared */
  comparedFields: Record<string, any>
}
