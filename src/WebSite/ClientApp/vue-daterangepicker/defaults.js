import moment from 'moment';

const FORMAT = 'MM/DD/YYYY'
export { FORMAT }

// Default range is one week
export default {
  startDate: moment().subtract(6, 'days').startOf('day').format(FORMAT),
  endDate: moment().endOf('day').format(FORMAT)
}